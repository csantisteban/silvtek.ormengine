echo off
echo ******* BUILD *********
%workspace%\NAnt\nant-0.86-beta1\bin\nant -buildfile:main.build 
pause