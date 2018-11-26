# Polyester-Yarn-Microscopic-Image-Data-Analysis <br />
Give a roughly or manually measurement of fibers from the microscopic image of the cross section of yarn (This application requires .NET 3.5/2.0/3.0 installed) <br />
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

There are various kind of edge detection algorithm such as Laplacian, Sobel, Kirsch and Prewitt. You can refer to this article by this link : https://softwarebydefault.com/2013/05/11/image-edge-detection/. We use two files **```Matrix.cs```** and **```ExtBitmap.cs```** provided by the website which I just mentioned. Some basic convolution method implemented in the edge detection can refer to this video: https://www.youtube.com/watch?v=XuD4C8vJzEQ and the following GIF from OpenCV. <br />

<p align="center">
<img src="/Image/readme/conv1.gif" height="50%" width="50%">   
  <!-- <img src="/Image/readme/conv.gif" height="25%" width="25%">   -->
</p>


In our application, we first apply **kirsh edge detection** on the selected image, and then we apply the **Prewitt edge detection** on the image which has been processed by the kirsh edge detection.  
<p align="center">
<img src="/Image/readme/Edgedetection.JPG" height="60%" width="60%">   
</p>
      
### Circle Detection For Unknown Radius <br />

We make a simple GIF in the following to demonstrate the algorithm of the Circle Hough Transform. In the GIF, we first determine the pixel which is marked as yellow dot. We make various radius of circles based on that yellow dot. We've already have some basic knowledge on the dimension of the circle in the given image, so we can narrow down the radius searching area for example from r = 10 to r = 15 (unit in pixel). The user can enter the MinR and MaxR to set the searching area. We will detect how many pixels which the value (either Green or Red or Blue value) is larger than a given threshold on the circle at given radius (we examine the pixel value every 3 degree on the circle to reduce the calculation time). The number of pixels (vote) should also be larger than a given threshold. </p>

In the following example, for yellow dot as the center of circle, we can see r = 10 (count=0), r = 11 (count=0), r = 12 (count=0), r = 13 (count=20), r = 14 (count=38), r = 15 (count=100). If we set the count (vote) threshold = 30, then only r = 14 and r = 15 will be taken into decision. We select the radius with the largest count (vote) number to be the radius of the center of the circle (the yellow dot in our case).  <br /> 

In another case, say the green dot as the center, we can see r = 10 (count=5), r = 11 (count=12), r = 12 (count=15), r = 13 (count=17), r = 14 (sorry, I forgot to make this one), r = 15 (count=21). If we set the count (vote) threshold = 30, then no radius will be taken into decision. Thus, there is no any circle at the green dot.  <br /> 

<p align="center">
<img src="/Image/readme/Circlealgorithm.gif" height="60%" width="60%">   
</p>

We make a little change on the vote system. When determine the radius, say at r = 13, we will also count the vote in r = 12 and r = 14 to r = 13. In this case, the vote number for r = 14 will contain the vote in r = 13 and r = 15. 

The count (vote) threshold is not a fix value. It will vary with the radius. The larger the radius is, the larger the threshold. The calculation for the threshold is ``` 3 * π * r * Accuracy ``` where the accuracy is given by the user. If the accuracy is set too high, then the application will hardly detect any circle (because the criteria is too high to define a circle). If the accuracy is set too low, then it will be susceptable to the noise and give many circles that are definetely not circles. <br /> 

### Merge Circle <br />

The application will examine every 4 columns of pixels in every 4 row of pixels. <br />

<p align="center">
<img src="/Image/readme/Detect.gif" height="25%" width="25%">   
 </p>       
       
      
# Some Cool Function <br />

### Draw line and measure on the Image and Zoom in <br />

There is a great post which I refer to : https://stackoverflow.com/questions/51138565/how-to-measure-length-of-line-which-is-drawn-on-image-c-sharp <br />




