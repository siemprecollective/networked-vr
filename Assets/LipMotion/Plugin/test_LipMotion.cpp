#include <iostream>

struct LipMotion_Point {
    int x, y;
};
extern "C" void *LipMotion_init(int camera, bool debugWindow);
extern "C" void LipMotion_destroy(void *lp);
extern "C" LipMotion_Point *LipMotion_processFrame();

int main(int argc, char** argv)
{
    void *lm = LipMotion_init(1, true);

    while(true) {
        auto frame = LipMotion_processFrame();
        for (int i=0; i<11; ++i) {
            std::cout << "(" << frame[i].x << ", " << frame[i].y << ") ";
        }
        std::cout << std::endl;
    }

    LipMotion_destroy(lm);
}
