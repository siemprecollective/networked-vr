#include <dlib/image_processing.h>
#include <dlib/gui_widgets.h>
#include <dlib/image_io.h>
#include <dlib/opencv/cv_image.h>

#include <opencv2/core.hpp>
#include <opencv2/videoio.hpp>

#include <iostream>
#include <thread>

using namespace dlib;
using namespace std;

// HELPERS

typedef scan_fhog_pyramid<pyramid_down<6>> image_scanner_type;
static object_detector<image_scanner_type> s_detector;
static shape_predictor s_predictor;
static cv::Mat s_frame;
static cv::VideoCapture s_videoCapture;
static image_window *s_debugWindow = nullptr;

static bool s_initialized = false;

inline std::vector<image_window::overlay_line> custom_render_face_detections(
    const std::vector<full_object_detection> &dets,
    const rgb_pixel color = rgb_pixel(0, 255, 0))
{
    std::vector<image_window::overlay_line> lines;
    for (unsigned long i = 0; i < dets.size(); ++i)
    {
        DLIB_CASSERT(dets[i].num_parts() == 11,
                     "\t std::vector<image_window::overlay_line> custom_render_face_detections()"
                         << "\n\t You have to give a 11 point"
                         << "\n\t dets[" << i << "].num_parts():  " << dets[i].num_parts());

        const full_object_detection &d = dets[i];

        lines.push_back(image_window::overlay_line(d.part(0), d.part(1), color));
        lines.push_back(image_window::overlay_line(d.part(1), d.part(3), color));
        lines.push_back(image_window::overlay_line(d.part(3), d.part(4), color));
        lines.push_back(image_window::overlay_line(d.part(4), d.part(5), color));
        lines.push_back(image_window::overlay_line(d.part(5), d.part(6), color));
        lines.push_back(image_window::overlay_line(d.part(6), d.part(7), color));
        lines.push_back(image_window::overlay_line(d.part(7), d.part(8), color));
        lines.push_back(image_window::overlay_line(d.part(8), d.part(0), color));
        lines.push_back(image_window::overlay_line(d.part(9), d.part(10), color));
        lines.push_back(image_window::overlay_line(d.part(10), d.part(2), color));
    }
    return lines;
}

inline std::vector<image_window::overlay_line> custom_render_face_detections(
    const full_object_detection &det,
    const rgb_pixel color = rgb_pixel(0, 255, 0))
{
    std::vector<full_object_detection> dets;
    dets.push_back(det);
    return custom_render_face_detections(dets, color);
}

// INTERFACE

extern "C" __declspec(dllexport) void *LipMotion_init(int camera, bool debugWindow)
{
    // TODO can't change camera
    // before the s_initialized to check to allow turning debugging on and off
    if (debugWindow)
    {
        s_debugWindow = new image_window();
    }

    if (s_initialized)
    {
        cerr << "LipMotion: already running" << endl;
        return (void *)1;
    }

    s_videoCapture.open(camera);
    if (!s_videoCapture.isOpened())
    {
        cerr << "LipMotion: error opening camera" << endl;
        return nullptr;
    }

    // TODO get resource
    deserialize("/Users/kumail/Dev/dlib/dataset_scripts/faces_closer_detector.svm") >> s_detector;
    deserialize("/Users/kumail/Dev/dlib/dataset_scripts/faces_closer.dat") >> s_predictor;

    s_initialized = true;
    return (void *)1;
}

extern "C" __declspec(dllexport) void LipMotion_destroy(void *lp)
{
    if (lp == nullptr)
        return;
    if (s_debugWindow)
        delete s_debugWindow;
    //s_initialized = false;
}

struct LipMotion_Point
{
    int x, y;
};
static LipMotion_Point s_processFrameOutput[11];
extern "C" __declspec(dllexport) LipMotion_Point *LipMotion_processFrame(void *lp)
{
    array2d<bgr_pixel> img;
    s_videoCapture.read(s_frame);
    if (s_frame.empty())
    {
        cout << "LipMotion: error reading frame!" << endl;
    }
    assign_image(img, cv_image<bgr_pixel>(s_frame));

    std::vector<rectangle> dets = s_detector(img);

    std::vector<full_object_detection> shapes;
    for (unsigned long j = 0; j < dets.size(); ++j)
    {
        full_object_detection shape = s_predictor(img, dets[j]);
        shapes.push_back(shape);
    }

    if (s_debugWindow != nullptr)
    {
        s_debugWindow->clear_overlay();
        s_debugWindow->set_image(img);
        s_debugWindow->add_overlay(custom_render_face_detections(shapes));
    }

    if (shapes.size() > 0)
    {
        auto shape = shapes[0];
        for (uint i = 0; i < shape.num_parts(); ++i)
        {
            s_processFrameOutput[i].x = shape.part(i).x();
            s_processFrameOutput[i].y = shape.part(i).y();
        }
    }

    return s_processFrameOutput;
}
