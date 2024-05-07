vec3 math_MulQuat(vec4 q, vec3 v){ 
	return v + 2 * cross(q.xyz, cross(q.xyz, v)+q.w*v);
}
