// Created by Crowbar 0.68

$modelname "Naval/Weapons/AA/Shell.mdl"

$staticprop

$bodygroup "Shell"
{
	studio "Shell.smd"
}


$surfaceprop "metalpanel"

$contents "solid"

$illumposition 0 -1.764 0

$cdmaterials "Naval\"

$cbox 0 0 0 0 0 0

$bbox -1.234 -1.234 -1.225 4.763 1.218 1.234

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"shell_anims\idle.smd"
	activity "ACT_IDLE" -1
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "shell_physics.smd"
{
	$mass 5
	$inertia 1
	$damping 1
	$rotdamping 1
	$rootbone " "
	$concave
	$maxconvexpieces 9

}
