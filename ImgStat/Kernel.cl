__kernel void satAvg(__read_only image2d_t src)
{
	const sampler_t samp = CLK_ADDRESS_CLAMP_TO_EDGE  |
	CLK_NORMALIZED_COORDS_FALSE |
	CLK_FILTER_NEAREST;
	int2 coord = (int2)(get_global_id(0), get_global_id(1));

};