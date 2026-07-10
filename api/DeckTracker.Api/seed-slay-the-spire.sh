#!/usr/bin/env bash
set -euo pipefail

BASE="${BASE:-http://localhost:5119}"

json_field() {
  # $1 = JSON string, $2 = field name
  node -e "console.log(JSON.parse(process.argv[1])[process.argv[2]])" "$1" "$2"
}

echo "Creating game..."
GAME=$(curl -s -X POST "$BASE/games" -H "Content-Type: application/json" -d '{
  "name": "Slay the Spire: The Board Game",
  "description": "Cooperative deckbuilder board game adaptation"
}')
GAME_ID=$(json_field "$GAME" id)
echo "  game id=$GAME_ID"

create_type() {
  local name="$1"
  local res
  res=$(curl -s -X POST "$BASE/games/$GAME_ID/card-types" -H "Content-Type: application/json" -d "{\"name\":\"$name\"}")
  json_field "$res" id
}

echo "Creating card types..."
TYPE_ATTACK=$(create_type "Attack")
TYPE_SKILL=$(create_type "Skill")
TYPE_POWER=$(create_type "Power")
TYPE_STATUS=$(create_type "Status")
TYPE_CURSE=$(create_type "Curse")
echo "  Attack=$TYPE_ATTACK Skill=$TYPE_SKILL Power=$TYPE_POWER Status=$TYPE_STATUS Curse=$TYPE_CURSE"

create_card() {
  local name="$1" typeId="$2" cost="$3"
  local body res
  if [ -z "$cost" ]; then
    body="{\"name\":\"$name\",\"typeId\":$typeId}"
  else
    body="{\"name\":\"$name\",\"typeId\":$typeId,\"cost\":$cost}"
  fi
  res=$(curl -s -X POST "$BASE/games/$GAME_ID/cards" -H "Content-Type: application/json" -d "$body")
  json_field "$res" id
}

echo "Creating cards..."
CARD_STRIKE=$(create_card "Strike" "$TYPE_ATTACK" 1)
CARD_DEFEND=$(create_card "Defend" "$TYPE_SKILL" 1)
CARD_BASH=$(create_card "Bash" "$TYPE_ATTACK" 2)
CARD_ANGER=$(create_card "Anger" "$TYPE_ATTACK" 0)
CARD_IRONWAVE=$(create_card "Iron Wave" "$TYPE_ATTACK" 1)
CARD_SHRUGITOFF=$(create_card "Shrug It Off" "$TYPE_SKILL" 1)
CARD_FLEX=$(create_card "Flex" "$TYPE_SKILL" 0)
CARD_DEMONFORM=$(create_card "Demon Form" "$TYPE_POWER" 3)
CARD_INFLAME=$(create_card "Inflame" "$TYPE_POWER" 1)
CARD_WOUND=$(create_card "Wound" "$TYPE_STATUS" "")
CARD_REGRET=$(create_card "Regret" "$TYPE_CURSE" "")
echo "  Strike=$CARD_STRIKE Defend=$CARD_DEFEND Bash=$CARD_BASH Anger=$CARD_ANGER Iron Wave=$CARD_IRONWAVE"
echo "  Shrug It Off=$CARD_SHRUGITOFF Flex=$CARD_FLEX Demon Form=$CARD_DEMONFORM Inflame=$CARD_INFLAME"
echo "  Wound=$CARD_WOUND Regret=$CARD_REGRET"

echo "Creating decks..."
curl -s -X POST "$BASE/games/$GAME_ID/decks" -H "Content-Type: application/json" -d "{
  \"result\": \"win\",
  \"character\": \"Ironclad\",
  \"notes\": \"Demon Form scaling run, act 3 boss down easily\",
  \"cards\": [
    {\"cardId\": $CARD_STRIKE, \"quantity\": 4},
    {\"cardId\": $CARD_DEFEND, \"quantity\": 3},
    {\"cardId\": $CARD_BASH, \"quantity\": 2},
    {\"cardId\": $CARD_IRONWAVE, \"quantity\": 2},
    {\"cardId\": $CARD_DEMONFORM, \"quantity\": 1},
    {\"cardId\": $CARD_INFLAME, \"quantity\": 2},
    {\"cardId\": $CARD_WOUND, \"quantity\": 1}
  ]
}" > /dev/null

curl -s -X POST "$BASE/games/$GAME_ID/decks" -H "Content-Type: application/json" -d "{
  \"result\": \"loss\",
  \"character\": \"Ironclad\",
  \"notes\": \"Too many curses, died to Act 2 elite\",
  \"cards\": [
    {\"cardId\": $CARD_STRIKE, \"quantity\": 5},
    {\"cardId\": $CARD_DEFEND, \"quantity\": 4},
    {\"cardId\": $CARD_ANGER, \"quantity\": 2},
    {\"cardId\": $CARD_SHRUGITOFF, \"quantity\": 1},
    {\"cardId\": $CARD_WOUND, \"quantity\": 2},
    {\"cardId\": $CARD_REGRET, \"quantity\": 2}
  ]
}" > /dev/null

echo "Done. Game id=$GAME_ID"
echo "  curl $BASE/games/$GAME_ID"
echo "  curl $BASE/games/$GAME_ID/analysis"
