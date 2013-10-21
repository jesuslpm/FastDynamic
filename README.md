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

To create an object a public default constructor must exist. 
The class however, doesn't have to be public, and it can be an anonymous type. 
Also, You need to know the type at runtime. 

You can use CreateObject extension method to create an object. If you are going to create only one object,
CreateObject is the preferred way.


<pre>
  Type type = GiveMeTheType();
  object obj = type.CreateObject();
</pre>

If you are going to create more than one object, it is better to use GetActivator extension method
 and use the returned activator multiple times.

<pre>
  Type type = GiveMeTheType();
  Func&lt;object&gt; activator = type.GetActivator();
  object obj1 = activator();
  object obj2 = activator();
  object obj3 = activator();
</pre>

Accessing properties and fields
-------------------------------

You can use FastDynamic to access not indexed public properties and fields.

If you are going to access the property or field only once, the preferred way is to use SetMemberValue 
and GetMemberValue extension methods:

<pre>
  object propValue = obj.GetMemberValue("PropertyName");
  object fieldValue = obj.GetMemberValue("FieldName");
  
  obj.SetMemberValue("PropertyName", somePropertyValue);
  obj.SetMemberValue("FieldName", someFieldValue);
</pre>

If you are going to access mutilple times, use getters and setters:

<pre>
  Type type = GiveMeTheType();
  IDictionary&lt;string, Setter&gt; setters = type.GetSetters();
  IDictionary&lt;string, Getter&gt; getters = type.GetGetters();
  
  Setter setter = setters["PropertyOrFieldName"];
  Getter getter = getters["PropertyOrFieldName"];
  
  setter(obj1, v1);
  setter(obj2, v2);
  setter(obj3, v3);
  
  
  object value1 = getter(obj1);
  object value2 = getter(obj2);
  object value3 = getter(obj3);
</pre>

GetGetters is an extension method that returns a dictionary. 
The dictionary keys are property or field names.
The dictionary values are Setters. 
A Setter is a delegate that is used to set the value of a property or field.


GetGetters is an extension method that returns a dictionary. 
The dictionary keys are property or field names.
The dictionary values are Getters. 
A getter is a delegate that is used to retrieve the value of a property or field.

