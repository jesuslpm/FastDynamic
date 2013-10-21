FastDynamic
===========

A fast alternative to reflection for creating objects and accesing properties and fields

Microbenchmark results:

Using reflection.. 1480 ms
Using CreateObject, GetMemberValue and SetMemberValue.. 621 ms
Using getters, setters and activator .. 85 ms
Known at compile time.. 9 ms