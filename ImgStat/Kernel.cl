__kernel void helloWorld(__global read_only int* message, int messageSize, image2d_t image)
{
	int4 pix = read_imagei(image, (int2)(0,0) );
	int lum = (0.2126*pix.x + 0.7152*pix.y + 0.0722*pix.z);
	for (int i = 0; i< messageSize; i++){
		printf("%d", message[i]);
	}

};