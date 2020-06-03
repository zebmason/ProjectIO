cmake_minimum_required (VERSION 3.8)
project(SourceGroups)

# this is not really a test which can fail
# it is more an example with several source_group()
# commands.
# The created projects have to be loaded manually
# in Visual Studio/Xcode/Eclipse/...
# to see whether the correct groups have been created.

# a sub group
source_group(Base\\Sub1       FILES $ENV{TEST_CMAKE_SUB1_PATH}/foo.c)

add_compile_definitions($ENV{TEST_CMAKE_CONSOLE})
add_executable(SourceGroups $ENV{TEST_CMAKE_SUB1_PATH}/foo.c)
