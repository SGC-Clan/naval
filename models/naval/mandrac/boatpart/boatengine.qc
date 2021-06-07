$modelname "mandrac/boatpart/boatengine.mdl"
$scale 0.1
$model "studio" "boatengine.smd"
$cdmaterials "mandrac\weapon"
$hboxset "default"
$surfaceprop "metal"
$keyvalues  { prop_data { "base" "Metal.Large"  } } 
$illumposition 0.000 -0.000 0.000
$sequence idle "boatengine_off.smd" fps 1.00
$sequence on "boatengine_on" fps 100.00
$collisionmodel "boatengine_phys.smd" 
{
$concave
$maxconvexpieces 54
$mass 100.00
$inertia 1.00
$damping 0.00
$rotdamping 0.00
}
