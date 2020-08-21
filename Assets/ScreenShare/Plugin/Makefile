OUTFILE = ScreenShare.dll

LIBS = \
	windowsapp.lib \
	d3d11.lib \
	d3dcompiler.lib \
	lib/parsec.lib

SOURCES = \
	src/EntryPoint.cpp \
	src/LocalScreen.cpp \
	src/RemoteScreenD3D11.cpp	

OBJECTS_1 = $(SOURCES:.cpp=.obj)
OBJECTS = $(OBJECTS_1:src/=obj/)

COMPILE_OPTS = /EHsc /std:c++17 /Zi
LINK_OPTS = /DLL /DEBUG:FASTLINK

{src}.cpp{obj}.obj ::
 	cl $(COMPILE_OPTS) /Fo:obj\ /c $<

dll: $(OBJECTS)
	link $(LINK_OPTS) $(LIBS) $** /out:lib/$(OUTFILE)

all: dll

clean:
	rm lib/$(OUTFILE)
	rm obj/*
