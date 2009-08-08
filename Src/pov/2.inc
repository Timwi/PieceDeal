#include "pdglobal.inc"

merge {

    sphere_sweep {
        b_spline
        11
        <3, 8, 0>, 1,
        <.7, 10, 0>, 1,
        <.7, 12.7, 0>, .7,
        <3, 14.3, 0>, .7,
        <7, 14.3, 0>, .7,
        <9.3, 12.5, 0>, .7,
        <9.3, 9, 0>, .7,
        <5, 6.5, 0>, .7,
        <.4, 3.5, 0>, .4,
        <.4, .8, 0>, .4,
        <.4, -1.9, 0>, .4
        tolerance .1
    }
    
    sphere_sweep {
        b_spline
        6
        <-1.4, .2, 0>, .8,
        <.8, .8, 0>, .8,
        <3, 1.4, 0>, .7,
        <7, 1.4, 0>, .7,
        <10, .4, 0>, .3,
        <13, -.6, 0>, .1
        tolerance .1
    }

    material { PDMaterial }
}

/*
sphere { <3, 8, 0>, .5      pigment { color Cyan } }
sphere { <.7, 10, 0>, .5    pigment { color Cyan } }
sphere { <.7, 12.7, 0>, .5  pigment { color Cyan } }
sphere { <3, 14.3, 0>, .5   pigment { color Cyan } }
sphere { <7, 14.3, 0>, .5   pigment { color Cyan } }
sphere { <9.3, 12.5, 0>, .5 pigment { color Cyan } }
sphere { <9.3, 9, 0>, .5    pigment { color Cyan } }
sphere { <5, 6.5, 0>, .5    pigment { color Cyan } }
sphere { <.4, 3.5, 0>, .5   pigment { color Cyan } }
sphere { <.4, .4, 0>, .5    pigment { color Cyan } }
sphere { <.4, -3, 0>, .5    pigment { color Cyan } }
*/

/*
sphere { <-1.9, .2, 0>, .5  pigment { color Cyan } }
sphere { <.8, .8, 0>, .5    pigment { color Cyan } }
sphere { <3.5, 1.4, 0>, .5  pigment { color Cyan } }
sphere { <7, 1.4, 0>, .5    pigment { color Cyan } }
sphere { <10, .4, 0>, .5    pigment { color Cyan } }
sphere { <13, -.6, 0>, .5   pigment { color Cyan } }
*/