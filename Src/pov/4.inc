#include "pdglobal.inc"

merge {

    sphere_sweep {
        b_spline
        5
        <3.3, 17, 0>, .9,
        <3.3, 15, 0>, .9,
        <2, 8, 0>, .8,
        <.7, 5, 0>, .7,
        <-.6, 2, 0>, .7
        tolerance .1
    }

    sphere_sweep {
        b_spline
        6
        <-1.3, 4.5, 0>, .7,
        <.7, 5, 0>, .7,
        <2.7, 5.5, -.11>, .7,
        <7, 5.5, -.11>, .6,
        <9.65, 5, 0>, .4,
        <12, 4.5, 0>, .3
        tolerance .1
    }

    sphere_sweep {
        b_spline
        5
        <6.6, 12, 0>, .7,
        <6.2, 10, 0>, .7,
        <6, 5.4, 0>, .7,
        <5.7, .5, 0>, .7,
        <5.7, -4.4, 0>, .7
        tolerance .1
    }

    material { PDMaterial }
}

/*
sphere { <2, 17, 0>, .5      pigment { color Cyan } }
sphere { <2, 15, 0>, .5      pigment { color Cyan } }
sphere { <1, 10, 0>, .5      pigment { color Cyan } }
sphere { <0, 5, 0>, .5       pigment { color Cyan } }
sphere { <-2, 3, 0>, .5      pigment { color Cyan } }

sphere { <-2, 4.5, 0>, .5    pigment { color Cyan } }
sphere { <0, 5, 0>, .5       pigment { color Cyan } }
sphere { <2, 5.5, 0>, .5     pigment { color Cyan } }
sphere { <7, 5.5, 0>, .5     pigment { color Cyan } }
sphere { <9.65, 5, 0>, .5    pigment { color Cyan } }
sphere { <12, 4.5, 0>, .5    pigment { color Cyan } }

sphere { <7, 12, 0>, .5      pigment { color Cyan } }
sphere { <6.4, 10, 0>, .5    pigment { color Cyan } }
sphere { <6, 5.4, 0>, .5     pigment { color Cyan } }
sphere { <5.4, .4, 0>, .5    pigment { color Cyan } }
sphere { <5.4, -4.6, 0>, .5  pigment { color Cyan } }
*/