FastDynamic
===========

A fast alternative to reflection for creating objects and accesing properties and fields.



Microbenchmark results
----------------------

Using reflection.. 1480 ms<br/>
Using CreateObject, GetMemberValue and SetMemberValue.. 621 ms<br/>
Using getters, setters and activator .. 85 ms<br/>
Known at compile time.. 9 ms<br/>

Creating objects
------------------

To create an object you need the type of the class at runtime and the class must have a public
default constructor. The class however, doesn't have to be public, and it can be an anonymous type.

You can use CreateObject extension method to create an object. If you are going to create only one object,
CreateObject is the prefered way.


<pre>
  Type type = GiveMeTheType();
  object obj = type.CreateObject();
</pre>

If you are going to create more than one object, it is better to use GetActivator extension method
to get the activator and use it multiple times.

<pre>
  Type type = GiveMeTheType();
  Func&lt;object&gt; activator = type.GetActivator();
  object obj1 = activator();
  object obj2 = activator();
  object obj3 = activator();
</pre>

Accessing properties and fields

