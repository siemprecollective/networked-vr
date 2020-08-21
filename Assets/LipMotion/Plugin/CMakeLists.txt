cmake_minimum_required(VERSION 2.8.12)
project(LipMotion)

include(FetchContent)
FetchContent_Declare(dlib
    GIT_REPOSITORY https://github.com/davisking/dlib.git
    # TODO we should specify a commit here
)
FetchContent_MakeAvailable(dlib)

cmake_policy(SET CMP0074 NEW)
if (DLIB_NO_GUI_SUPPORT)
   message(FATAL_ERROR "Dlib must be built with GUI support!")
else()
   find_package(OpenCV QUIET)
   if (OpenCV_FOUND)
      include_directories(${OpenCV_INCLUDE_DIRS})

      add_library(LipMotion SHARED LipMotion.cpp)
      target_link_libraries(LipMotion dlib::dlib ${OpenCV_LIBS} )
      
      add_executable(test_LipMotion test_LipMotion.cpp)
      target_link_libraries(test_LipMotion LipMotion dlib::dlib ${OpenCV_LIBS} )
   else()
      message(FATAL_ERROR "Set OpenCV_ROOT to the path of your OpenCV Installation")
   endif()
endif()
