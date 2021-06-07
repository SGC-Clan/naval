// Created by Crowbar 0.19.0.0

$modelname "mandrac/boatpart/boatengine.mdl"

$bodygroup "studio"
{
	studio "boatengine_reference.smd"
}


$cdmaterials "mandrac\weapon\"

$surfaceprop "metal"

$contents "solid"

$hboxset "default"




$sequence "idle" "boatengine_anims\idle.smd" {
	fps 1
}

$sequence "on" "boatengine_anims\on.smd" {
	fps 100
}

$collisionmodel "boatengine_physics.smd"
{
	$mass 100
	$inertia 1
	$damping 0
	$rotdamping 0
	$concave

}


$keyvalues
{
	prop_data 
	{
		"base" "Metal.Large"
	}
}
