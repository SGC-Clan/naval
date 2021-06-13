// Created by Crowbar 0.68

$modelname "props\props\cball.mdl"

$staticprop

$bodygroup "Body"
{
	studio "cball.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0.025 -0.01 -0.023

$cdmaterials "models\props\"

$cbox 0 0 0 0 0 0

$bbox -4.029 -3.959 -3.998 4.049 4.009 3.952

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"cball_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 15
	loop
}

$collisionmodel "cball_physics.smd"
{
	$mass 100
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
	$concave
	$maxconvexpieces 1

}

$keyvalues
{
	prop_data 
	{
		"base"		"Metal.Medium"
	}
}
