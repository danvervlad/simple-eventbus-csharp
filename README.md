# Simple EventBus

Simple EventBus implementation in Csharp

## EventBus usage examples

EventBus has simple api to ```Send``` event in specific topic with data and ```Subscribe``` on it.

### Create

```c#
var eventBus = new EventBus();
```

### Subscribe

```c#
eventBus.Subscribe("topic", "eventName", data => Console.WriteLine(data));
```

### Send
It will be sent immediately

```c#
eventBus.Send("topic", "eventName", "payloadData");
```

### Post
It will be placed in queue and sent when ```eventBus.ProceedQueue()``` will be called, for example in game loop.

```c#
eventBus.Post("topic", "eventName", "payloadData");
```

## Requester usage examples

Requester is a decorator for EventBus. The main purpose of it is to make async call (```Request```) via EventBus. On the other side should be done ```SubscribeForReply```. Only first replier can return result.

### Create instance

```c#
var requester = new Requester(new EventBus());
```

### SubscribeForReply

The Handler returns ```Task<T>```, that means any async operation could done there. 

```c#
requester.SubscribeForReply<string>("topic", "eventName", (data, token) => // return task with result here);
```
### Request
Request reply result and wait for it.

```c#
var result = await requester.Request<string>("topic", "eventName", "payloadData");
```

## EventBus Facade

Facade which combines EventBus and Requester in one interface.

### EventBusFacade usage examples

Same as for previous two