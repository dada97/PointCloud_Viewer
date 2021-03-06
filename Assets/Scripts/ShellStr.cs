﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellStr : MonoBehaviour
{
    public static string FirstHalf = "SECONDS=0 # clear timer\n"
     + "cd PointcloudToTextureMesh/combinePly\n"
     + "python3 combinePly.py ../../temp/ICP/ ../../temp/Poisson/Poisson.ply\n"
     + "ComSeconds=$SECONDS\n"
     + "\n"
     + "SECONDS=0 # clear timer\n"
     + "cd ../PoissonReconstruction/\n"
     + "Bin/Linux/PoissonRecon --in ../../temp/Poisson/Poisson.ply --out ../../temp/Poisson/PoissonMesh.ply --colors --depth 6\n"
     + "xvfb-run -a meshlabserver -i ../../temp/Poisson/PoissonMesh.ply -o ../../temp/Poisson/PoissonMesh.obj -s ../clearNonManifold.mlx -om vn vc\n"
     + "xvfb-run -a meshlabserver -i ../../temp/Poisson/PoissonMesh.obj -o ../../temp/Poisson/PoissonMesh.obj -s ../clearNonManifold.mlx -om vn vc\n"
     + "xvfb-run -a meshlabserver -i ../../temp/Poisson/PoissonMesh.obj -o ../../temp/Poisson/Binary.ply -s ../clearNonManifold.mlx -om vn vc\n"
     + "ReconSeconds=$SECONDS\n"
     + "\n"
     + "SECONDS=0 # clear timer\n"
     + "cd ../\n"
     + "python3 toAsciiPly.py ../temp/Poisson/Binary.ply ../temp/Poisson/Ascii.ply\n"
     + "python3 PoissonClean.py ../temp/ICP/ ../temp/Poisson/Ascii.ply ../temp/Poisson/Ascii.ply\n"
     + "xvfb-run -a meshlabserver -i ../temp/Poisson/Ascii.ply -o ../temp/Poisson/Ascii.obj -s ./clearNonManifold.mlx -om vn vc\n"
     + "xvfb-run -a meshlabserver -i ../temp/Poisson/Ascii.obj -o ../temp/Poisson/Binary.ply -s ./clearNonManifold.mlx -om vn vc\n"
     + "python3 toAsciiPly.py ../temp/Poisson/Binary.ply ../temp/Poisson/Ascii.ply\n"
     + "ClearSeconds=$SECONDS\n"
     + "\n"
     + "SECONDS=0 # clear timer\n"
     + "cd ../Optcuts/\n"
     + "timeout 5m ./build/OptCuts_bin 100 ../temp/Poisson/Ascii.obj ../temp/Poisson/Optcuts.obj\n"
     + "OptSeconds=$SECONDS\n"
     + "\n"
     + "SECONDS=0 # clear timer\n"
     + "cd ../PointcloudToTextureMesh/SampleUV/\n"
     + "python3 Run.py ../../temp/ICP/ ../../temp/data/Output_UV_PointCloud/ ../../temp/UVPC/\n"
     + "ComUVSeconds=$SECONDS\n"
     + "\n"
     + "cd /home/PointcloudToTextureMesh/SampleUV\n"
     + "\n"
     + "SECONDS=0 # clear timer\n"
     + "python3 SampleUV.py --UVpath ../../temp/UVPC/ --TexturePath ../../temp/data/Output_Camera_1080/ --MaskPath ../../temp/data/Output_Binary_Mask/ --PoissonName ../../temp/Poisson/Ascii.ply --TextPath ../../temp/data/Output_SLAM/CameraTrajectory_angle.txt -o ../../temp/Poisson/Sample --fileName ";
    public static string SecondHalf = "\n"
        + "SampleUVSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd /home/GenerateTexture/GenerateTextureByMapping_Cuda/\n"
        + "./a.out ../../temp/Poisson/Sample.obj ../../temp/Poisson/Optcuts.obj ../../temp/Poisson/Sample.png ../../temp/Poisson/Optcuts.png ../../temp/Poisson/OptcutsMask.png ../../temp/Poisson/OptcutsMask2.png\n"
        + "\n"
        + "cd ../GenerateTextureByColor_Cuda/\n"
        + "./a.out ../../temp/Poisson/Ascii.obj ../../temp/Poisson/Optcuts.obj ../../temp/Poisson/Color.png\n"
        + "\n"
        + "cd ../../TexturePostProcessing/combine/\n"
        + "./Combine ../../temp/Poisson/Optcuts.png ../../temp/Poisson/Color.png ../../temp/Poisson/OptcutsMask.png ../../temp/Poisson/Combine.png\n"
        + "\n"
        + "cd ../SeamRepair/\n"
        + "./SeamRepair ../../temp/Poisson/Combine.png ../../temp/Poisson/OptcutsMask2.png 10 ../../temp/Poisson/Seam.png\n"
        + "\n"
        + "cd ../Filter/\n"
        + "./Filter ../../temp/Poisson/Seam.png ../../temp/Poisson/OptcutsMask.png ../../temp/Poisson/Filter.png 5 5 3\n"
        + "TexSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../../MeshSimplification/\n"
        + "./decimater ../temp/Poisson/Optcuts.obj num-vertices 1000\n"
        + "SimSeconds=$SECONDS\n"
        + "\n"
        + "\n"
        + "\n"
        + "\n"
        + "echo \"Combine Ply: $ComSeconds seconds\"\n"
        + "echo \"Poisson Reconstrucion: $ReconSeconds seconds\"\n"
        + "echo \"Mesh Clearing: $ClearSeconds seconds\"\n"
        + "echo \"Optcuts Ply: $OptSeconds seconds\"\n"
        + "echo \"Combine UVPly: $ComUVSeconds seconds\"\n"
        + "echo \"Sample UV: $SampleUVSeconds seconds\"\n"
        + "echo \"Texture Processing: $TexSeconds seconds\"\n"
        + "echo \"Mesh Simplifying: $SimSeconds seconds\"\n";

    public static string BoxFirstHalf = "SECONDS=0 # clear timer\n"
        + "cd PointcloudToTextureMesh/combinePly\n"
        + "python3 combinePly.py ../../temp/ICP/ ../../temp/Poisson/Poisson.ply\n"
        + "ComSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../PoissonReconstruction/\n"
        + "Bin/Linux/PoissonRecon --in ../../temp/Poisson/Poisson.ply --out ../../temp/Poisson/PoissonMesh.ply --colors --depth 6\n"
        + "xvfb-run -a meshlabserver -i ../../temp/Poisson/PoissonMesh.ply -o ../../temp/Poisson/PoissonMesh.obj -s ../clearNonManifold.mlx -om vn vc\n"
        + "xvfb-run -a meshlabserver -i ../../temp/Poisson/PoissonMesh.obj -o ../../temp/Poisson/PoissonMesh.obj -s ../clearNonManifold.mlx -om vn vc\n"
        + "xvfb-run -a meshlabserver -i ../../temp/Poisson/PoissonMesh.obj -o ../../temp/Poisson/Binary.ply -s ../clearNonManifold.mlx -om vn vc\n"
        + "ReconSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../\n"
        + "python3 toAsciiPly.py ../temp/Poisson/Binary.ply ../temp/Poisson/Ascii.ply\n"
        + "python3 PoissonClean.py ../temp/ICP/ ../temp/Poisson/Ascii.ply ../temp/Poisson/Ascii.ply\n"
        + "xvfb-run -a meshlabserver -i ../temp/Poisson/Ascii.ply -o ../temp/Poisson/Ascii.obj -s ./clearNonManifold.mlx -om vn vc\n"
        + "xvfb-run -a meshlabserver -i ../temp/Poisson/Ascii.obj -o ../temp/Poisson/Binary.ply -s ./clearNonManifold.mlx -om vn vc\n"
        + "python3 toAsciiPly.py ../temp/Poisson/Binary.ply ../temp/Poisson/Ascii.ply\n"
        + "ClearSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../Optcuts/\n"
        + "timeout 5m ./build/OptCuts_bin 100 ../temp/Poisson/Ascii.obj ../temp/Poisson/Optcuts.obj\n"
        + "OptSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../MeshSimplification/\n"
        + "./decimater ../temp/Poisson/Optcuts.obj num-vertices 1000\n"
        + "SimSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../PointcloudToTextureMesh/SampleUV/\n"
        + "python3 Run.py ../../temp/ICP/ ../../temp/data/Output_UV_PointCloud/ ../../temp/UVPC/\n"
        + "ComUVSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../Delaunay/ \n"
        + "python3 RunDelaunay.py ../../temp/UVPC/ ../../temp/data/Output_Camera_1080/ ../../temp/data/Output_Binary_Mask/ ../../temp/Delaunay/\n"
        + "DelSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../SampleUV\n"
        + "python3 RunSampleUVByDelaunay.py  -ip ../../temp/ICP -pp ../../temp/Poisson/Ascii.obj -dp ../../temp/Delaunay -tp ../../temp/data/Output_Camera_1080 -mp ../../temp/data/Output_Binary_Mask -o ../../temp/Sample -r 0.004\n"
        + "python3 RunMapping.py -mp ../../GenerateTexture/GenerateTextureByMapping_Cuda/a.out -op ../../temp/Poisson/Optcuts.obj -ip ../../temp/ICP -sp ../../temp/Sample\n"
        + "SampleSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "cd ../Texture\n"
        + "python3 RunTextureSmooth.py  -ip ../../temp/ICP -sp ../../temp/Sample\n"
        + "SmoothSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "python3 TextureCombine.py --imgPath ../../temp/Sample/ --outPath ../../temp/Poisson/Optcuts_new --fileName ";
    public static string BoxSecondHalf = "\n"
        + "ComTexSeconds=$SECONDS\n"
        + "\n"
        + "SECONDS=0 # clear timer\n"
        + "python3 TextureInpainting.py --imgPath ../../temp/Poisson/ --outPath ../../temp/Poisson/Optcuts_inpainting.png --fileName Optcuts_new\n"
        + "TexSeconds=$SECONDS\n"
        + "\n"
        + "echo \"Combine Ply: $ComSeconds seconds\"\n"
        + "echo \"Poisson Reconstrucion: $ReconSeconds seconds\"\n"
        + "echo \"Mesh Clearing: $ClearSeconds seconds\"\n"
        + "echo \"Optcuts Ply: $OptSeconds seconds\"\n"
        + "echo \"Combine UVPly: $ComUVSeconds seconds\"\n"
        + "echo \"Delaunay Triangulation: $DelSeconds seconds\"\n"
        + "echo \"Sample UV: $SampleSeconds seconds\"\n"
        + "echo \"Texture Smoothing: $SmoothSeconds seconds\"\n"
        + "echo \"Mesh Simplifying: $SimSeconds seconds\"\n"
        + "echo \"Texture Combine: $ComTexSeconds seconds\"\n"
        + "echo \"Texture Processing: $TexSeconds seconds\"\n";
    public static string BoxRename = "mv temp/ICP temp/ICP_ori\n"
        + "mkdir temp/ICP\n";
}
