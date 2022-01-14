@echo off 
set message=git rev-parse --abbrev-ref HEAD
echo "current branch: "
echo %message%

git push

git checkout origin master
git pull

git merge %message%
git push origin master

git checkout %message%