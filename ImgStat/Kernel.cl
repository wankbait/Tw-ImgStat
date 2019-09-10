__kernel void helloWorld(__global read_only int* message, int messageSize)
{

	for (int i = 0; i< messageSize; i++){
		printf("%d", message[i]);
	}

};