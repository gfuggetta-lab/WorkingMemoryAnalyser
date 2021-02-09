rm -rf diskimage
mkdir diskimage
cp -r "../../scms_project_1.app" "./diskimage/Working Memory Analyser.app"
cp -r "../../Sounds" "./diskimage/Working Memory Analyser.app/Contents/Resources/"
mkdir "./diskimage/Working Memory Analyser.app/Contents/Frameworks"
cp -r ~/Library/Frameworks/SDL2.framework "./diskimage/Working Memory Analyser.app/Contents/Frameworks/"

mkdir "./diskimage/Experiment Library"
cp -r "../../Experiment Library/Working Memory Load Experiment 1" "./diskimage/Experiment Library/"
cp -r "../../Experiment Library/Working Memory Load Experiment 2" "./diskimage/Experiment Library/"
dmgbuild -s settings.py "Working Memory Analyser" "Working Memory Analyser.dmg"