
camera { location <5, 7.5, -20> look_at <5, 7.5, 0> }
// camera { location <0, 0, -5> look_at <0, 0, 0> }

#declare PDMaterial = material {
    texture {
        pigment { color rgbt<1.2, .8, 0, .125> }
        normal { wrinkles .1 scale 1 }
        finish { phong .5 }
    }
}

#declare PDDebugMaterial = material {
    texture {
        pigment { color rgbt<1.2, .8, 0, .5> }
        normal { wrinkles .1 scale 1 }
        finish { phong .5 }
    }
}
