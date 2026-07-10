param(
    [string]$Base = "http://localhost:5119"
)

$ErrorActionPreference = "Stop"

Write-Host "Creating game..."
$game = Invoke-RestMethod -Uri "$Base/games" -Method Post -ContentType "application/json" -Body (@{
    name        = "Slay the Spire: The Board Game"
    description = "Cooperative deckbuilder board game adaptation"
} | ConvertTo-Json)
$gameId = $game.id
Write-Host "  game id=$gameId"

function New-CardType([string]$Name) {
    $res = Invoke-RestMethod -Uri "$Base/games/$gameId/card-types" -Method Post -ContentType "application/json" -Body (@{ name = $Name } | ConvertTo-Json)
    return $res.id
}

Write-Host "Creating card types..."
$typeAttack = New-CardType "Attack"
$typeSkill  = New-CardType "Skill"
$typePower  = New-CardType "Power"
$typeStatus = New-CardType "Status"
$typeCurse  = New-CardType "Curse"
Write-Host "  Attack=$typeAttack Skill=$typeSkill Power=$typePower Status=$typeStatus Curse=$typeCurse"

function New-Card([string]$Name, [int]$TypeId, [Nullable[int]]$Cost) {
    $body = @{ name = $Name; typeId = $TypeId }
    if ($null -ne $Cost) { $body.cost = $Cost }
    $res = Invoke-RestMethod -Uri "$Base/games/$gameId/cards" -Method Post -ContentType "application/json" -Body ($body | ConvertTo-Json)
    return $res.id
}

Write-Host "Creating cards..."
$cardStrike      = New-Card "Strike" $typeAttack 1
$cardDefend      = New-Card "Defend" $typeSkill 1
$cardBash        = New-Card "Bash" $typeAttack 2
$cardAnger       = New-Card "Anger" $typeAttack 0
$cardIronWave    = New-Card "Iron Wave" $typeAttack 1
$cardShrugItOff  = New-Card "Shrug It Off" $typeSkill 1
$cardFlex        = New-Card "Flex" $typeSkill 0
$cardDemonForm   = New-Card "Demon Form" $typePower 3
$cardInflame     = New-Card "Inflame" $typePower 1
$cardWound       = New-Card "Wound" $typeStatus $null
$cardRegret      = New-Card "Regret" $typeCurse $null
Write-Host "  Strike=$cardStrike Defend=$cardDefend Bash=$cardBash Anger=$cardAnger IronWave=$cardIronWave"
Write-Host "  ShrugItOff=$cardShrugItOff Flex=$cardFlex DemonForm=$cardDemonForm Inflame=$cardInflame"
Write-Host "  Wound=$cardWound Regret=$cardRegret"

Write-Host "Creating decks..."
Invoke-RestMethod -Uri "$Base/games/$gameId/decks" -Method Post -ContentType "application/json" -Body (@{
    result    = "win"
    character = "Ironclad"
    notes     = "Demon Form scaling run, act 3 boss down easily"
    cards     = @(
        @{ cardId = $cardStrike; quantity = 4 }
        @{ cardId = $cardDefend; quantity = 3 }
        @{ cardId = $cardBash; quantity = 2 }
        @{ cardId = $cardIronWave; quantity = 2 }
        @{ cardId = $cardDemonForm; quantity = 1 }
        @{ cardId = $cardInflame; quantity = 2 }
        @{ cardId = $cardWound; quantity = 1 }
    )
} | ConvertTo-Json -Depth 5) | Out-Null

Invoke-RestMethod -Uri "$Base/games/$gameId/decks" -Method Post -ContentType "application/json" -Body (@{
    result    = "loss"
    character = "Ironclad"
    notes     = "Too many curses, died to Act 2 elite"
    cards     = @(
        @{ cardId = $cardStrike; quantity = 5 }
        @{ cardId = $cardDefend; quantity = 4 }
        @{ cardId = $cardAnger; quantity = 2 }
        @{ cardId = $cardShrugItOff; quantity = 1 }
        @{ cardId = $cardWound; quantity = 2 }
        @{ cardId = $cardRegret; quantity = 2 }
    )
} | ConvertTo-Json -Depth 5) | Out-Null

Write-Host "Done. Game id=$gameId"
Write-Host "  Invoke-RestMethod $Base/games/$gameId"
Write-Host "  Invoke-RestMethod $Base/games/$gameId/analysis"
