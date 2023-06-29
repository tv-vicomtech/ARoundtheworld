# Orkestralib-Unity

This repository contains the Unity implementation of the Orkestra client. **OrkestraLib** is a solution developed by Vicomtech for multi-device applications.
This is a port of (a part of) **OrkestraLib** for the Unity framework. Its main objective is to make the work of developers easier by
simplifying the network functionalities as much as possible. In this sense, **OrkestraLib** uses the websocket architecture to ensure communication
between users and it abstracts all network difficulties, making it easy to share data between users in real time.

## Server

OrkestraLib backend is a _NodeJS_ web server with a MongoDB data base. Its main functionality is the orchestration of the
different connections based on websockets. It offers the posibility of saving data on session and recovering these data after the
communication is closed. For the moment, you can use https://arete-orkestralib.vicomtech.org/ for testing purposes. This repository **DOES NOT** include the OrkestraLib backend

## Client

OrkestraLib client is the part where hard work is done. It abstracts all websocket asynchronization problems, trying to share data between different clients.

## Getting Ready

In case you want to use it, you should import the latest version of the unity-package of Orkestra (available in the latest [release](https://github.com/tv-vicomtech/orkestralib-unity/releases)) into your Unity project. This
source depends on the C# SocketioClient library, which is included in the package.

## API

The first thing to do is to import Orkestra into your application and then instantiate an Orkestra object:

### Creating an instance

```c#
 using OrkestraLib;
 using OrkestraLib.Message;
 using static OrkestraLib.Orkestra;

 ...

 orkestra = GetComponent<OrkestraWithSocketIO>();

```

After that, you need to specify some required parameters. These could be hardcoded, provided externally (e.g., from a text file) or even specified by the user when starting the application.

### Configuring the main parameters

Room (session name), the url (url of the server) and the agentid (username)

```c#
// orkestra.url = "https://cloud.flexcontrol.net";
orkestra.url = "https://arete-orkestralib.vicomtech.org";
orkestra.agentId = "user1";
orkestra.room = "classA";
```

Orkestra allows exchanging data of any type, provided that the data has been registered:

### Registering the keys of the data that are going to be shared

This can be done through an object defined within `/Scripts/Messages`:

```c#
orkestra.RegisterEvents(new Type[]{
	typeof(ActiveUser),
	typeof(Question),
	typeof(Answer),
	typeof(Result),
	typeof(CameraView),
	typeof(Notification),
	typeof(SceneView)
 });
```

Or through a string:

```c#
orkestra.RegisterEvent("data");
```

The data can then be exchanged through the User Context (that is, sending data to a specific user) or through the Application Context (that is, sending a broadcast notification of a state change):

### Subscribing to events of User or Application Context

For the user context:

```c#

/*Register the user context subscriber*/
orkestra.UserEvents+=UserEventSubscriber;

/* Receive user context events */
void UserEventSubscriber(object sender, UserEvent evt)
{
/* associate the event to an action in the app */
Debug.Log("UserEventSubscriber(" + evt.ToJSON() + ")");
}
```

For the application context:

```c#
/*Register the application context subscriber*/
ork.AppEvents+= AppEventSubscriber;

/* Receive application context events*/
void AppEventSubscriber(object sender, AppEvent evt)
{
/* associate the event to an action in the app */
Debug.Log("AppEventSubscriber(" + evt.ToJSON() + ")");
}
```

### Sending info to a specific user

If an object is shared: user channel is used with a message of the corresponding type and the user to which the message is sent. Only the specified user will receive the message.

```c#
 orkestra.Dispatch(Channel.User, new Message(sender, value), user);
```

If a string is shared:

```c#
 orkestra.SetUserItem(string toAgentId, string key, string value);
```

### Sending a message globally through the Application Context

If an object is shared: application channel is used with a message of the corresponding type. All the users connected to the same session will receive the message.

```c#
 /* Parameters: */
 orkestra.Dispatch(Channel.Application, new Message(sender, value));
```

If a string is shared:

```c#
 orkestra.SetAppAttribute(string key, string value);
```

### Reset the room

Reset the room when the user disconnects:

```c#
orkestra.ResetRoomAtDisconnect=true;
```

Remove all keys:

```c#
orkestra.RemoveAllKeys();
```

Remove a specific key:

```c#
orkestra.RemoveKey("name_of_the_key");
```

## Compatibility

This orkestraLib port can be used with the exiting Orkestra JS library to share data between web browsers and Unity applications.
