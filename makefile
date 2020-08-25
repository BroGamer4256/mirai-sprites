MAIN_FILE = Program

CSHARP_SOURCE_FILES = $(wildcard */*/*.cs */*.cs *.cs)

CSHARP_FLAGS = -out:$(EXECUTABLE)

CSHARP_COMPILER = mcs

EXECUTABLE = $(MAIN_FILE).exe

RM_CMD = -rm -f $(EXECUTABLE)

all: $(EXECUTABLE)

$(EXECUTABLE): $(CSHARP_SOURCE_FILES)
	@ $(CSHARP_COMPILER) $(CSHARP_SOURCE_FILES) $(CSHARP_FLAGS)

run: all
	@ mono $(EXECUTABLE)

clean:
	@ $(RM_CMD)

remake:
	@ $(MAKE) clean
	@ $(MAKE)
