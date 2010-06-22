echo off
echo ORM ENGINE
%workspace%\NAnt\nant-0.86-beta1\bin\nant -buildfile:build.xml assembly.build
pause