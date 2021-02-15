rm -rf diskimage
mkdir diskimage
cp -r "../../scms_project_1.app" "./diskimage/Working Memory Analyser.app"
rm    "./diskimage/Working Memory Analyser.app/Contents/MacOS/scms_project_1"
strip "../../scms_project_1"
cp    "../../scms_project_1" "./diskimage/Working Memory Analyser.app/Contents/MacOS/"
cp -r "../../Sounds" "./diskimage/Working Memory Analyser.app/Contents/Resources/"
mkdir "./diskimage/Working Memory Analyser.app/Contents/Frameworks"
cp -r /Library/Frameworks/SDL2.framework "./diskimage/Working Memory Analyser.app/Contents/Frameworks/"
cp -r /Library/Frameworks/SDL2_mixer.framework "./diskimage/Working Memory Analyser.app/Contents/Frameworks/"
cp -r /Library/Frameworks/SDL2_ttf.framework "./diskimage/Working Memory Analyser.app/Contents/Frameworks/"

mkdir "./diskimage/Experiment Library"
cp -r "../../Experiment Library/Exp 01 Working Memory Capacity" "./diskimage/Experiment Library/"
cp -r "../../Experiment Library/Exp 02 Working Memory Load and Distractor processing" "./diskimage/Experiment Library/"
dmgbuild -s settings.py "Working Memory Analyser" "Working Memory Analyser.dmg"