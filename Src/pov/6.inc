
#include "pdglobal.inc"
#include "colors.inc"

merge {

    sphere_sweep {
        b_spline
        15
        <9.7, 12.4, 0>, 1
        <8.8, 13.4, 0>, 1
        <6.9, 14.4, 0>, .7
        <3.2, 13.8, 0>, .7
        <.7, 9.7, 0>, .6
        <.7, 3.5, 0>, .6
        <3, .7, 0>, .7
        <6.55, .7, 0>, .7
        <9.2, 3, 0>, .8
        <9.3, 7.3, 0>, .75
        <7.3, 9.8, 0>, .7
        <4, 9.8, 0>, .7
        <1.85, 7.5, 0>, .5
        <1.85, 5, 0>, .3
        <2.6, 2.7, 0>, .1
        tolerance .1
    }

    material { PDMaterial }
}

/*
#declare CyanTrans = rgbt<0, 1, 1, .5>;
sphere { <9.7, 12.4, 0>, .5   pigment { color CyanTrans } }
sphere { <8.8, 13.4, 0>, .5   pigment { color CyanTrans } }
sphere { <6.9, 14.4, 0>, .5   pigment { color CyanTrans } }
sphere { <3.2, 13.8, 0>, .5   pigment { color CyanTrans } }
sphere { <.7, 9.7, 0>, .5     pigment { color CyanTrans } }
sphere { <.7, 3.5, 0>, .5     pigment { color CyanTrans } }
sphere { <3, .7, 0>, .5       pigment { color CyanTrans } }
sphere { <6.55, .7, 0>, .5    pigment { color CyanTrans } }
sphere { <9.2, 3, 0>, .5      pigment { color CyanTrans } }
sphere { <9.3, 7.3, 0>, .5    pigment { color CyanTrans } }
sphere { <7.3, 9.8, 0>, .5    pigment { color CyanTrans } }
sphere { <4, 9.8, 0>, .5      pigment { color CyanTrans } }
sphere { <1.85, 7.5, 0>, .5   pigment { color CyanTrans } }
sphere { <1.85, 5, 0>, .5     pigment { color CyanTrans } }
sphere { <2.6, 2.7, 0>, .5    pigment { color CyanTrans } }
* */
