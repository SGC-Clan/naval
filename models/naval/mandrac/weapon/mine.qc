$modelname "mandrac/boatpart/mine.mdl"
$scale 0.1
$model "studio" "mine.smd"
$cdmaterials "mandrac\weapon"
$hboxset "default"
$surfaceprop "metal"
$keyvalues  { prop_data { "base" "Metal.Large"  } } 
$illumposition 0.000 -0.000 0.000
$sequence idle "mine_idle" fps 100.00
$collisionmodel "mine_phys.smd" 
{
$mass 100.00
$inertia 1.00
$damping 0.00
$rotdamping 0.00
}
