# Polyester-Yarn-Microscopic-Image-Data-Analysis <br />
Give a roughly or manually measurement of fibers from the microscopic image of the cross section of yarn (This application requires .NET 3.5/2.0/3.0 installed) <br />
<p align="center">
<img src="/Image/Version3_2.gif" height="80%" width="80%"> 
</p>  

 
<p align="center">
GIF 1. All the function are demonstrated here. 
</p>
 

# Function of the Project <br />

### Auto Detection <br />

* Detect each fiber in the yarn automatically and give a roughly measurement of the diameter (unit in pixel) in the cross section of the fiber. <br />

<p align="center">
<img src="/Image/readme/Autodetect.JPG" height="90%" width="90%">   
</p>

<p align="center">
Image 1. Circles auto detection
</p>

<p align="center">
<img src="/Image/Version3_2Autofast.gif" height="80%" width="80%"> 
</p>  

 
<p align="center">
GIF 2. Auto Detection. 
</p>

There are four parameters which require specify by the user. <br />
```Min R``` and ```Max R``` are for definding the searching space. <br />
```PixThred``` is the threshold of pixels value which the user want to detect (The value ranges from 0 to 255. RGB value are the same because we are analyzing the grayscale image). <br />
```Accuracy``` is to define what the criteria is to define a circle. The smaller the accuracy, the application will be more susceptable to the noise and give some circle with only a few white dot on that circle. However, if you set the accuracy too high, the application will hardly give all the circle detection. <br />
```Small``` is the ratio compares with the largest circle (marked in blue) which the application can detect in the image. If any circle compares with the largest one smaller than the ratio, will be marked in red. Other circle will be marked in green. <br />

### Manual Measurement <br />

* Provide an user interface to manually measure the width in the cross section of the fiber. <br />
<p align="center">
<img src="/Image/readme/Manualdetect.JPG" height="90%" width="90%">   
</p>

<p align="center">
Image 2. Circles manual detection
</p>

# About the Theory <br />

The gist of this application is circular object detection. The more percise term is Circle Hough Transform. First, we need to make the edge more clear from the image. Second, we need to detect the circle. <br />

The whole process can be summrized into the following flow chart. Each step will be specified in the following paragraph<br />
<p align="center">
<img src="/Image/readme/FlowChart1.png" height="95%" width="95%">   
</p>

<p align="center">
Image 3. Flow chart of the algorithm
</p>
        
### Image Edge Detection <br />

There are various kind of edge detection algorithm such as Laplacian, Sobel, Kirsch and Prewitt. You can refer to this article by this link : https://softwarebydefault.com/2013/05/11/image-edge-detection/. We use two files **```Matrix.cs```** and **```ExtBitmap.cs```** provided by the website which I just mentioned. Some basic convolution method implemented in the edge detection can refer to this video: https://www.youtube.com/watch?v=XuD4C8vJzEQ and the following GIF from OpenCV. <br />

<p align="center">
<img src="/Image/readme/conv1.gif" height="50%" width="50%">   
  <!-- <img src="/Image/readme/conv.gif" height="25%" width="25%">   -->
</p>

<p align="center">
GIF 3. Convolution (By OpenCV)
</p>


In our application, we first apply **kirsh edge detection** on the selected image, and then we apply the **Sobel edge detection** on the image which has been processed by the kirsh edge detection.  
<p align="center">
<img src="/Image/readme/Edgedetection1.png" height="70%" width="70%">   
</p>

<p align="center">
Image 4. Image processing from the original image to gray style with edge detection
</p>
      
### Circle Detection For Unknown Radius <br />

The application will examine the pixel every 4 columns for every 4 rows in the image showing in the following GIF. The pixel marked with yellow will be regarded as the center of a circle.<br />

<p align="center">
<img src="/Image/readme/Detect.gif" height="25%" width="25%">   
</p>   

<p align="center">
GIF 4. The examine steps of the image (also see GIF 7).
</p>
     
We make a simple GIF in the following to demonstrate the algorithm of the Circle Hough Transform. In the GIF, we first determine the pixel which is marked as yellow dot. We make various radius of circles based on that yellow dot. <br />
 

We've already have some basic knowledge on the dimension of the circle in the given image, so we can narrow down the radius searching space for example from r = 10 to r = 15 (unit in pixel). The user can enter the MinR and MaxR to set the searching space. We will detect how many pixels which the  pixel value (either Green or Red or Blue value) is larger than a given threshold on the virtual circle at given radius (we examine the pixel value every 3 degree on the virtual circle to reduce the calculation time). The number of pixels (vote) should also be larger than a given threshold. </p>

<p align="center">
<img src="/Image/readme/Circlealgorithm.gif" height="60%" width="60%">   
</p>

<p align="center">
GIF 5. Circle detects based on certain pixel as the center of virtual circle
</p>

In the above example, for yellow dot as the center of virtual circle, we can see r = 10 (count=0), r = 11 (count=0), r = 12 (count=0), r = 13 (count=20), r = 14 (count=38), r = 15 (count=100). If we set the count (vote) threshold = 30, then only r = 14 and r = 15 will be taken into decision. We select the radius with the largest count (vote) number to be the radius of the center of the circle (the yellow dot in our case).  <br /> 

In another case, say the green dot as the center, we can see r = 10 (count=5), r = 11 (count=12), r = 12 (count=15), r = 13 (count=17), r = 14 (sorry, I forgot to make this one), r = 15 (count=21). If we set the count (vote) threshold = 30, then no radius will be taken into decision. Thus, there is no any circle at the green dot.  <br /> 

The voting process is illustrated in the following GIF. The yellow dot is the center of various virtual circles. The red and the green dots are the examination point. If the red or the green point detects the white pixel when passing it, the vote will plus one for the current radius. <br /> 

<p align="center">
<img src="/Image/readme/examine.gif" height="45%" width="45%">   
</p> 

<p align="center">
GIF 6. Voting Process ( Searching space r = 3 ~ r = 6 )
</p>

We make a little change on the vote system. When determine the radius, say at r = 13, we will also count the vote in r = 12 and r = 14 to r = 13. In this case, the vote number for r = 14 will contain the vote in r = 13 and r = 15. 

The count (vote) threshold is not a fix value. It will vary with the radius. The larger the radius is (thus longer circumference), the larger the threshold should be. The calculation for the threshold is ``` [ 2 * π * ( r - 1  + r + r + 1 ) * Accuracy / 3 ] * [ 3 / 2 ]  ==>  3πr *  Accuracy``` where the accuracy is given by the user. <br />

<p align="center">
<img src="/Image/readme/threshold.JPG" height="65%" width="65%">  
 </p>
 

 
If the accuracy is set too high, then the application will hardly detect any circle (because the criteria is too high to define a circle). If the accuracy is set too low, then it will be susceptable to the noise and give many circles that are definetely not circles. The following image demonstrate the relationship between the accuracy and the result of circle detection. Circles in the Image 3 have been merged. The term "Misdetect" means the algorithm thinks there is a circle, but the truth is that there is no circle. <br /> 

<p align="center">
<img src="/Image/readme/Accurelation.png" height="85%" width="85%">  
 </p>
 
<p align="center">
Image 5. The relationship of the accuracy and the result of circle detection.
</p>

### Merge Circle <br />
 
 The algorithm might draw many circles which are pretty close to the ground truth. The merge process is to determine which circles should be combined together because they are indicating the same ground truth. Circles which are close enough to each other will be assigned to the same group. We define ``` Distance of two center of distinct circles < ( radius of one circle + radius of another circle ) / 2 ``` as close to each other. The determine process is illustrated in the following GIF. We examine each of the circles provided by the Circle Hough Transform Algorithm. Thus, the number of groups is equal to the number of circles.  <br />
 
 <p align="center">
<img src="/Image/readme/Merge-FirstStep1.gif" height="65%" width="65%">  
 </p> 
 
<p align="center">
GIF 7. Define the group of circles
</p>

The circles in the same group will be combined to one circle. We use the concept of center of mass to claculate the center of merged circle ( x <sub>merged</sub> , y <sub>merged</sub> ) and the radius ( r <sub>merged</sub> ).  <br />


 r <sub>merged</sub> = ( r <sub>1</sub> + r <sub>2</sub> + r <sub>3</sub> + ..... ) / number of circles in the group  <br />
 x <sub>merged</sub> = ( x <sub>1</sub> *  r <sub>1</sub> + x <sub>2</sub> * r <sub>2</sub> + x <sub>3</sub> * r <sub>3</sub> + ..... ) / ( r <sub>1</sub> + r <sub>2</sub> + r <sub>3</sub> + ..... )  <br />
 y <sub>merged</sub> = ( y <sub>1</sub> *  r <sub>1</sub> + y <sub>2</sub> * r <sub>2</sub> + y <sub>3</sub> * r <sub>3</sub> + ..... ) / ( r <sub>1</sub> + r <sub>2</sub> + r <sub>3</sub> + ..... )  <br /> 

We do the merge process twice to further combine the circles and the result does improve. Take the GIF 8 for example, Group 1, Group 2, and Group 3 will form three separate merged circles. However, they are indicating the same ground truth. Thus, we require another merging process to further combine these three circles which are so close to each other. <br /> 

# Some Cool Function <br />

### Make Heavy Work in The Background <br />
For those who are new to Windows form might be surprised that their application just hang there without updating any textboxs or labels untill all the calculation are over. With the BackgroundWorker class, we can easily update the progressbar as well as other variable showing on the form and your application will not freeze when you try to click other buttons during the heavy calculation of the application. Here are two links for your reference : https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.backgroundworker?redirectedfrom=MSDN&view=netframework-4.7.2  and https://www.wpf-tutorial.com/misc/multi-threading-with-the-backgroundworker/ <br />

The following gif demonstrates the function of applying the BackgroundWorker class in the form. <br />

<p align="center">
<img src="/Image/BackgroundWork.gif" height="80%" width="80%">  
 </p> 
 
<p align="center">
GIF 9. Application with BackgroundWork
</p>

### Devides Image into Three Parts and Process them together<br />

Our image will be devided into three parts like the following image when the auto measurement started. Three backgroundworkers will be initialized for processing these three parts individually so that we can speed up the calculation time.The overlape is to make sure those circles on the edge will be detected correctly.

<p align="center">
<img src="/Image/readme/Devide3D.png" height="75%" width="75%">  
 </p>
 
<p align="center">
Image 6. The image will be devided into three overlapped parts and processed by three backgroundworkers.
</p>

        
      

### Zoom in to Draw Line and Measure on the Image <br />

There is a great post which I refer to : https://stackoverflow.com/questions/51138565/how-to-measure-length-of-line-which-is-drawn-on-image-c-sharp <br />

Notice that you should first create a panel and make the AutoSize properties to False and the AutoScroll properties to True. Second, put the PictureBox on this panel (Do not make the PictureBox size bigger than the panel). Set the SizeMode of the PictureBox to Zoom. Follow the steps in the post and then you can have the zoom in and zoom out function. Maybe the image in the PictureBox changes the position after the first zoom in or out. You can put the following code when loading the image so that the image will only change the position for this time without zooming the image ( the user will not notice that). <br />

```C#
float zoom = 1f;
pictureBox1.Image = Image.FromFile(opbl.FileName);  // opbl is OpenFileDialog
int w = (int)(pictureBox2.Image.Width * zoom);
int h = (int)(pictureBox2.Image.Height * zoom);
pictureBox1.ClientSize = new Size(w, h);                
```


### UI Design <br />

I pretty like the UI design by the following youtube link. <br /> 
https://www.youtube.com/watch?v=nLfzH4xOVqo


# Problem Need to be Solved <br />

The edge detection result is not ideal for some of the pictures given by our quality control department which lead to poor circle detection. Some bad image conversion result are as the following. <br />

<p align="center">
<img src="/Image/readme/bad3.JPG" height="75%" width="75%">  
</p>


      

