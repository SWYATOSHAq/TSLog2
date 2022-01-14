@echo off 
set message=git rev-parse --abbrev-ref HEAD
echo "current branch: "
echo %message%

git checkout master
git pull origin/master

git merge %message%
git push origin/master

git checkout %message%