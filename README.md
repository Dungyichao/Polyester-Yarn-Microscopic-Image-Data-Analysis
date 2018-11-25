# Polyester-Yarn-Microscopic-Image-Data-Analysis <br />
Give a roughly or manually measurement of fibers from the microscopic image of the cross section of yarn <br />
<p align="center">
<img src="/Image/All1.gif" height="80%" width="80%"> 
</p>  

# Function of the Project <br />

### Auto Detection <br />

* Detect each fiber in the yarn automatically and give a roughly measurement of the width in the cross section of the fiber. <br />
<p align="center">
<img src="/Image/Auto1.gif" height="80%" width="80%"> 
</p>

### Manual Measurement <br />

* Provide an user interface to manually measure the width in the cross section of the fiber. <br />
<p align="center">
<img src="/Image/Manual4.gif" height="80%" width="80%"> 
</p>

# About the Theory <br />

The gist of this application is circular object detection. The more percise term is Circle Hough Transform. First, we need to make the edge more clear from the image. Second, we need to detect the circle. <br />

### Image Edge Detection <br />

There are various kind of edge detection algorithm such as Laplacian, Sobel, Kirsch and Prewitt. You can refer to this article by this link : https://softwarebydefault.com/2013/05/11/image-edge-detection/. We use two files **```Matrix.cs```** and **```ExtBitmap.cs```** provided by the website which I just mentioned. Some basic convolution method can refer to this video: https://www.youtube.com/watch?v=XuD4C8vJzEQ<br />

<p align="left">
<img src="/Image/readme/conv1.gif" height="70%" width="70%">                
</p>

<p align="right">
<img src="/Image/readme/conv.gif" height="70%" width="70%">                
</p>

In our application, we first apply **kirsh edge detection** on the selected image, and then we apply the **Prewitt edge detection** on the image which has been processed by the kirsh edge detection.  
