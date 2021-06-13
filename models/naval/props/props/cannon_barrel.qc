// Created by Crowbar 0.68

$modelname "props\props\cannon_barrel.mdl"

$bodygroup "studio"
{
	studio "cannon_barrel.smd"
}

$lod 24
{
	replacemodel "cannon_barrel.smd" "cannon_barrel_lod1.smd"
	nofacial
}
$lod 48
{
	replacemodel "cannon_barrel.smd" "cannon_barrel_lod2.smd"
	nofacial
}
$shadowlod
{
	replacemodel "cannon_barrel.smd" "cannon_barrel_lod3.smd"
	nofacial
}

$surfaceprop "metal"

$contents "solid"

$illumposition -3.58 0 2.276

$cdmaterials "models\props\"

$attachment "wick" "wick04" 0 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -15.873 -55.2 -8.747 15.873 48.22 13.299

$definebone "barrel_bone" "" 0 0 0 0 0 0 0 0 0 0 0 0
$definebone "wick01" "barrel_bone" 33.344296 0.07755 7.132946 0 0 0 0 0 0 0 0 0
$definebone "wick02" "wick01" -0.040787 -0.034944 2.244698 0 0 0 0 0 0 0 0 0
$definebone "wick03" "wick02" -0.169876 0.000129 2.309755 0 0 0 0 0 0 0 0 0
$definebone "wick04" "wick03" 0.024872 0.034944 2.370562 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"cannon_barrel_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$sequence "spentidle" {
	"cannon_barrel_anims\spentidle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$sequence "ignite" {
	"cannon_barrel_anims\ignite.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "ready" {
	"cannon_barrel_anims\ready.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "cannon_barrel_physics.smd"
{
	$mass 300
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
