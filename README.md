# Simple EventBus

Simple EventBus implementation in Csharp

## Usage examples

### Create instance

```c#
var eventBus = new EventBus();
```

### Subscribe

```c#
eventBus.Subscribe("topic", "eventName", data => Console.WriteLine(data));
```

### Send event
It will be sent immediately

```c#
eventBus.Send("topic", "eventName", "payloadData");
```

### Post event
It will be placed in queue and sent when ```eventBus.ProceedQueue()``` will be called, for example in game loop.

```c#
eventBus.Post("topic", "eventName", "payloadData");
```