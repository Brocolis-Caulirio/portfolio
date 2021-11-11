float uInvLerp( float lowerBound, float upperBound, float value)
{
    return (value - lowerBound) / (upperBound - lowerBound);
}
float invLerp( float lowerBound, float upperBound, float value)
{
    float val = (value - lowerBound) / (upperBound - lowerBound);
    
    val = upperBound > lowerBound ? min(upperBound, max(lowerBound, val)) : min(lowerBound, max(upperBound, val));
    return val;
}

float4 debugKRGBW(float val, float low, float up)
{
    float range = (up-low)/3;
    float3 red =    (val < low + range      && val > low ) * val * float3(0,1,0);
    float3 green =  (val < low + 2 * range  && val > low + range) * val * float3(0, 1, 1);
    float3 blue =   (val < up               && val > low + 2 * range) * val * float3(0,0,1);
    float3 white =  (val > 1) * float3(.75,1,0);
    float3 black =  (val < 0) * abs(val) * float3(.5,0,1);
    float4 lightmapped = float4(red + green + blue + white, 1);

    return lightmapped;

}

//weird toons
float toon(float3 input, float ToonShades)
{
    input *= ToonShades/2.;
    float3 difInt = floor(input);
    float3 difFrac = frac(input);

    float derivative = 1/ToonShades/2;
    difFrac = uInvLerp( derivative , 1-derivative, difFrac);
    input = (difInt + difFrac) / (ToonShades/2);    

    return input;
}
float toonShadow(float3 input, float ToonShades)
{
    input *= ToonShades/2.;
    float3 difInt = floor(input);
    return 1-(difInt/ToonShades/2);

    float3 difFrac = frac(input);
    float diffChange = fwidth(input);

    float derivative = 1/ToonShades/2;
    float3 lowerBound = 0;
    float3 upperBound = diffChange;
    difFrac = uInvLerp( 1-derivative , derivative, difFrac);
    input = (difInt + difFrac) / (ToonShades/2);    

    return input;
}

//good toons
float2 fwidthLU(float i)
{
    float lower = i - fwidth(i) / 2;
    float upper = i + fwidth(i) / 2;
    return float2(lower, upper);
}
float pencilToon(float i, int count)
{

    if (count <= 1)
        return floor(i);

    //return floor(i * count) / count;

    float mul = floor(i * count) / count;
    float check = (fwidth(mul) > 1 / count);
    float icheck = (check - 1) * -1;
    float2 lu = fwidthLU(mul);
    //only works bc all values are unchanging until they change
    //in essence it only makes a 2x2 pixel transition line instead of immediate transition

    mul = (check * invLerp(lu.x, lu.y, i)) + (icheck * mul);
    return mul;

}
float smoothToon(float i, float count)
{

    if (count <= 1)
        return floor(i);
    //return floor(i * count) / count;
    float MD = .125;

    float edge = count;
    float mul = floor(i * count);

    float Frac = frac(i * count);
    float q = 1. / count;

    if (Frac < MD)
        mul = lerp(mul - q, mul, Frac / MD);
    if (Frac > 1 - MD)
        mul = lerp(mul + q, mul, (1 - Frac) / MD);

    mul = mul / count;

    return mul + (Frac / 10);

}
float smoothToon(float i, float count, float mergeDist)
{

    if (count <= 1 || (mergeDist > .5 || mergeDist < 0))
        return floor(i);
    //return floor(i * count) / count;
    float MD = mergeDist;

    float edge = count;
    float mul = floor(i * count);

    float Frac = frac(i * count);
    float q = 1. / count;

    if (MD>0 && Frac < MD)
        mul = lerp(mul - q, mul, Frac / MD);
    if (MD>0 && Frac > 1 - MD)
        mul = lerp(mul + q, mul, (1 - Frac) / MD);

    mul = mul / count;

    return mul + (Frac / 10);

}

float3x3 TBN_maker(float3 tangent, float3 bitangent, float3 normal)
{
    float3x3 m_TangentToWorld = {
        tangent.x, bitangent.x, normal.x,
        tangent.y, bitangent.y, normal.y,
        tangent.z, bitangent.z, normal.z,
    };
    return m_TangentToWorld;
}

float LightIntensity(float4 LightColor)
{
    return LightColor/normalize(LightColor);
}
