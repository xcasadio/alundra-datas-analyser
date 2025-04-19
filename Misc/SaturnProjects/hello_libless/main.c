#include <math.h>

#define ENV_BITS	10
#define ENV_LEN		(1<<ENV_BITS)
#define ENV_STEP	(128.0/ENV_LEN)

int main(void)
{
	double div = (1) * (ENV_STEP/4.0) / 8.0;
	long p = pow(2, (1) * (ENV_STEP/4.0) / 8.0);
	double m = (1<<16) / pow(2, (1) * (ENV_STEP/4.0) / 8.0);
	long mm = p;
	
	
	
	
}

