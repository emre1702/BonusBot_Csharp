NOCOLOR='\033[0m'
RED='\033[0;31m'
GREEN='\033[0;32m'
ORANGE='\033[0;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
LIGHTGRAY='\033[0;37m'
DARKGRAY='\033[1;30m'
LIGHTRED='\033[1;31m'
LIGHTGREEN='\033[1;32m'
YELLOW='\033[1;33m'
LIGHTBLUE='\033[1;34m'
LIGHTPURPLE='\033[1;35m'
LIGHTCYAN='\033[1;36m'
WHITE='\033[1;37m'
SEPERATOR='==============================='

cd /cygdrive/b/Users/EmreKara/Desktop/Tools/GitHub/BonusBot/build

echo -e "${SEPERATOR}"
echo -e "Update ${LIGHTBLUE}Bonusbot ${NOCOLOR}..."
rsync -hmrtvzP --chmod=Du=rwx,Dgo=rw,Fu=rw,Fog=r --timeout=60 --delete --exclude="*.config" --exclude="*.db" -e "B:\cygwin64\bin\ssh.exe -p 55555 -i C:/Users/emre1/.ssh/mvs-root" . bonusbot@185.101.94.212:/home/bonusbot

cmd /k