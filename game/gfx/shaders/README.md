The file extensions used for shaders are:

| Shader stage | extension |
| ------------ | --------- |
| Vertex | .vert |
| Fragment | .frag |
| Compute | .comp |

Shared files end with .inc and are not compiled. Shared files used by specific stages should use the extension `<stage extension>.inc`.
For example: `GammaCorrection.frag.inc`.
Compiled files add .spv to the extension.
