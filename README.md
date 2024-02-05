# SatelliteRpc
[TOC]

## 项目信息

### 编译环境

要求 .NET 7.0 SDK 版本，Visual Studio 和 Rider 对应版本都可以。

### 目录结构
```
├─samples                   // 示例项目
│  ├─Client                 // 客户端示例
│  │  └─Rpc                 // RPC客户端服务
│  └─Server                 // 服务端示例
│      └─Services           // RPC服务端服务
├─src                       // 源代码
│  ├─SatelliteRpc.Client    // 客户端
│  │  ├─Configuration       // 客户端配置信息
│  │  ├─Extensions          // 针对HostBuilder和ServiceCollection的扩展
│  │  ├─Middleware          // 客户端中间件，包含客户端中间件的构造器
│  │  └─Transport           // 客户端传输层，包含请求上下文，默认的客户端和Rpc链接的实现
│  ├─SatelliteRpc.Client.SourceGenerator    // 客户端代码生成器,用于生成客户端的调用代码
│  ├─SatelliteRpc.Protocol  // 协议层，包含协议的定义，协议的序列化和反序列化，协议的转换器
│  │  ├─PayloadConverters   // 承载数据的序列化和反序列化，包含ProtoBuf
│  │  └─Protocol            // 协议定义，请求、响应、状态和给出的Login.proto
│  ├─SatelliteRpc.Server    // 服务端
│  │  ├─Configuration       // 服务端配置信息，还有RpcServer的构造器
│  │  ├─Exceptions          // 服务端一些异常
│  │  ├─Extensions          // 针对HostBuilder、ServiceCollection、WebHostBuilder的扩展
│  │  ├─Observability       // 服务端的可观测性支持，目前实现了中间件
│  │  ├─RpcService          // 服务端的具体Rpc服务的实现
│  │  │  ├─DataExchange     // 数据交换，包含Rpc服务的数据序列化
│  │  │  ├─Endpoint         // Rpc服务的端点，包含Rpc服务的端点，寻址，管理
│  │  │  └─Middleware       // 包含Rpc服务的中间件的构造器
│  │  └─Transport           // 服务端传输层，包含请求上下文，服务端的默认实现，Rpc链接的实现，链接层中间件构建器
│  └─SatelliteRpc.Shared    // 共享层，包含一些共享的类
│      ├─Application        // 应用层中间件构建基类，客户端和服务端中间件构建都依赖它
│      └─Collections        // 一些集合类
└─tests                     // 测试项目
    ├─SatelliteRpc.Protocol.Tests
    ├─SatelliteRpc.Server.Tests
    └─SatelliteRpc.Shared.Tests
```
## 设计方案
下面简单的介绍一下总体的设计方案：

### 传输协议设计
传输协议的主要代码在`SatelliteRpc.Protocol`项目中，协议的定义在`Protocol`目录下。针对RPC的请求和响应创建了两个类，一个是`AppRequest`另一个是`AppResponse`。

在代码注释中，描述了协议的具体内容，这里简单的介绍一下，请求协议定义如下：
```text
[请求总长度][请求Id][请求的路径(字符串)]['\0'分隔符][请求数据序列化类型][请求体]
```
响应协议定义如下：
```text
[响应总长度][请求Id][响应状态][响应数据序列化类型][响应体]
```
其中主要的参数和数据在各自请求响应体中，请求体和响应体的序列化类型是通过`PayloadConverters`中的序列化器进行序列化和反序列化的。

在响应时使用了**请求Id**，这个请求Id是ulong类型，是一个**链接唯一的**自增的值，每次请求都会自增，这样就可以保证每次请求的Id都是唯一的，这样就可以在客户端和服务端进行匹配，从而找到对应的请求，从而实现多路复用的请求和响应匹配功能。

当ulong类型的值超过最大值时，会从0开始重新计数，由于ulong类型的值是64位的，值域非常大，所以在正常的情况下，同一连接下不可能出现请求Id重复的情况。

### 客户端设计

客户端的层次结构如下所示，最底层是传输层的中间件，它由`RpcConnection`生成，它用于TCP网络连接和最终的发送接受请求，中间件构建器保证了它是整个中间件序列的最后的中间件，然后上层就是用户自定义的中间件。

默认的客户端实现`DefaultSatelliteRpcClient`，目前只提供了几个Invoke方法，用于不同传参和返参的服务，在这里会执行中间件序列，最后就是具体的`LoginClient`实现，这里方法定义和`ILoginClient`一致，也和服务端定义一致。

最后就是调用的代码，现在有一个`DemoHostedService`的后台服务，会调用一下方法，输出日志信息。

下面是一个层次结构图：
```text
[用户层代码]
    |
[LoginClient]
    |
[DefaultSatelliteRpcClient]
    |
[用户自定义中间件]
    |
[RpcConnection]
    |
[TCP Client]
```
所以整个RCP Client的关键实体的转换如下图所示：
```text
请求：[用户PRC 请求响应契约][CallContext - AppRequest&AppResponse][字节流]
响应：[字节流][CallContext - AppRequest&AppResponse][用户PRC 请求响应契约]
```
#### 多路复用

上文提到，多路复用主要是使用ulong类型的Id来匹配Request和Response，主要代码在`RpcConnection`，它不仅提供了一个最终用于发送请求的方法，
在里面声明了一个`TaskCompletionSource`的字典，用于存储请求Id和`TaskCompletionSource`的对应关系，这样就可以在收到响应时，通过请求Id找到对应的`TaskCompletionSource`，从而完成请求和响应的匹配。

由于请求可能是并发的，所以在`RpcConnection`中声明了`Channel<AppRequest>`，将并发的请求放入到Channel中，然后在`RpcConnection`中有一个后台线程，用于从Channel单线程的中取出请求，然后发送请求，避免并发调用远程接口时，底层字节流的混乱。

#### 扩展性

客户端不仅仅支持`ILoginClient`这一个契约，用户可以自行添加其他契约，只要保障服务端有相同的接口实现即可。也支持增加其它proto文件，Protobuf.Tools会自动生成对应的实体类。

##### 中间件
该项目的扩展性类似ASP.NET Core的中间件，可以自行加入中间件处理请求和响应，中间件支持Delegate形式，也支持自定义中间件类的形式，如下代码所示：
```csharp
public class MyMiddleware : IRpcClientMiddleware
{
    public async Task InvokeAsync(ApplicationDelegate<CallContext> next, CallContext next)
    {
        // do something
        await next(context);
        // do something
    }
}
```
在客户端中间件中，可以通过`CallContext`获取到请求和响应的数据，然后可以对数据进行处理，然后调用`next`方法，这样就可以实现中间件的链式调用。

同样也可以进行阻断操作，比如在某个中间件中，直接返回响应，这样就不会继续调用后面的中间件；或者记录请求响应日志，或者进行一些其他的操作，类似于ASP.NET Core中间件都可以实现。

##### 序列化
序列化的扩展性主要是通过`PayloadConverters`来实现的，内部实现了抽象了一个接口`IPayloadConverter`，只要实现对应PayloadType的序列化和反序列化方法即可，然后注册到DI容器中，便可以使用。

由于时间关系，只列出了Protobuf和Json两种序列化器，实际上可以支持用户自定义序列化器，只需要在请求响应协议中添加标识，然后由用户注入到DI容器即可。

##### 其它

其它一些类的实现基本都是通过接口和依赖注入的方式实现，用户可以很方便的进行扩展，在DI容器中替换默认实现即可。如：`IRpcClientMiddlewareBuilder`、
`IRpcConnection`、`ISatelliteRpcClient`等。

另外也可以自行添加其他的服务，因为代码生成器会自动扫描接口，然后生成对应的调用代码，所以只需要在接口上添加`SatelliteRpcAttribute`，声明好方法契约，就能实现。

### 服务端设计

服务端的设计总体和客户端设计差不多，中间唯一有一点区别的地方就是服务端的中间件有两种：
- 一种是针对连接层的`RpcConnectionApplicationHandler`中间件，设计它的目的主要是为了灵活处理链接请求，由于可以直接访问原始数据，还没有做路由和参数绑定，后续可观测性指标和一些性能优化在这里做会比较方便。
  - 比如为了应对RPC调用，定义了一个名为`RpcServiceHandler`的`RpcConnectionApplicationHandler`中间件，放在整个连接层中间件的最后，这样可以保证最后执行的是RPC Service层的逻辑。
- 另外一种是针对业务逻辑层的`RpcServiceMiddleware`，这里就是类似ASP.NET Core的中间件，此时上下文中已经有了路由信息和参数绑定，可以在这做一些AOP编程，也能直接调用对应的服务方法。
  - 在RPC层，我们需要完成路由，参数绑定，执行目标方法等功能，这里就是定义了一个名为`EndpointInvokeMiddleware`的中间件，放在整个RPC Service层中间件的最后，这样可以保证最后执行的是RPC Service层的逻辑。

下面是一个层次结构图：
```text
[用户层代码]
    |
[LoginService]
    |
[用户自定义的RpcServiceMiddleware]
    |
[RpcServiceHandler]
    |
[用户自定义的RpcConnectionApplicationHandler]
    |
[RpcConnectionHandler]
    |
[Kestrel]
```
整个RPC Server的关键实体的转换如下图所示：
```text
请求：[字节流][RpcRawContext - AppRequest&AppResponse][ServiceContext][用户PRC Service 请求契约]
响应：[用户PRC Service 响应契约][ServiceContext][AppRequest&AppResponse][字节流]
```
#### 多路复用

服务端对于多路复用的支持就简单的很多，这里是在读取到一个完整的请求以后，直接使用Task.Run执行后续的逻辑，所以能做到同一链接多个请求并发执行，
对于响应为了避免混乱，使用了`Channel<HttpRawContext>`，将响应放入到Channel中，然后在后台线程中单线程的从Channel中取出响应，然后返回响应。

#### 终结点

在服务端中有一个终结点的概念，这个概念和ASP.NET Core中的概念类似，它具体的实现类是`RpcServiceEndpoint`；在程序开始启动以后；
便会扫描入口程序集（当然这块可以优化），然后找到所有的`RpcServiceEndpoint`，然后注册到DI容器中，然后由`RpcServiceEndpointDataSource`统一管理，
最后在进行路由时有`IEndpointResolver`根据路径进行路由，这只提供了默认实现，用户也可以自定义实现，只需要实现`IEndpointResolver`接口，然后替换DI容器中的默认实现即可。

#### 扩展性

服务端的扩展性也是在**中间件**、**序列化**、**其它接口**上，可以通过DI容器很方便的替换默认实现，增加AOP切面等功能，也可以直接添加新的Service服务，因为会默认去扫描入口程序集中的`RpcServiceEndpoint`，然后注册到DI容器中。

## 优化

现阶段做的性能优化主要是以下几个方面：
- Pipelines
  - 在客户端的请求和服务端处理(Kestrel底层使用)中都使用了Pipelines，这样不仅可以降低编程的复杂性，而且由于直接读写Buffer，可以减少内存拷贝，提高性能。
- 表达式树
  - 在动态调用目标服务的方法时，使用了表达式树，这样可以减少反射的性能损耗，在实际场景中可以设置一个快慢阈值，当方法调用次数超过阈值时，就可以使用表达式树来调用方法，这样可以提高性能。
- 代码生成
  - 在客户端中，使用了代码生成技术，这个可以让用户使用起来更加简单，无需理解RPC的底层实现，只需要定义好接口，然后使用代码生成器生成对应的调用代码即可；另外实现了客户端自动注入，避免运行时反射注入的性能损耗。
- 内存复用
  - 对于RPC框架来说，最大的内存开销基本就在请求和响应体上，创建了PooledArray和PooledList，两个池化的底层都是使用的ArrayPool，请求和响应的Payload都是使用的池化的空间。 
- 减少内存拷贝
  - RPC框架消耗CPU的地方是内存拷贝，上文提到了客户端和服务端均使用Pipelines，在读取响应和请求的时候直接使用`ReadOnlySequence<byte>`读取网络层数据，避免拷贝。
  - 客户端请求和服务端响应创建了PayloadWriter类，通过`IBufferWriter<byte>`直接将序列化的结果写入网络Buffer中，减少内存拷贝，虽然会引入闭包开销，但是相对于内存拷贝来说，几乎可以忽略。
  - 对于这个优化实际应该设置一个阈值，当序列化的数据超过阈值时，才使用PayloadWriter，否则使用内存拷贝的方式，需要Benchmark测试支撑阈值设置。

其它更多的性能优化需要Benchmark的数据支持，由于时间比较紧，没有做更多的优化。

## 待办

计划做，但是没有时间去实现的：

- 服务端代码生成
  - 现阶段服务端的路由是通过字典匹配实现，方法调用使用的表达式树，实际上这一块可以使用代码生成来实现，这样可以提高性能。
  - 另外一个地方就是Endpoint注册是通过反射扫描入口程序集实现的，实际上这一步可以放在编译阶段处理，在编译时就可以读取到所有的服务，然后生成代码，这样可以减少运行时的反射。
- 客户端取消请求
  - 目前客户端的请求取消只是在客户端本身，取消并不会传递到服务端，这一块可以通过协议来实现，在请求协议中添加一个标识，传递Cancel请求，然后在服务端进行判断，如果是取消请求，则服务端也根据ID取消对应的请求。
- Context 和 AppRequest\AppResponse 池化
  - 目前的Context和AppRequest\AppResponse都是每次请求都会创建，对于这些小对象可以使用池化的方式来实现复用，其中AppRequest、AppResponse已经实现了复用的功能，但是没有时间去实现池化，Context也可以实现池化，但是目前没有实现。
- 堆外内存、FOH管理
  - 目前的内存管理都是使用的堆内存，对于那些有明显作用域的对象和缓存空间可以使用堆外内存或FOH来实现，这样可以减少GC在扫描时的压力。
- AsyncTask的内存优化
  - 目前是有一些地方使用的ValueTask，对于这些地方也是内存分配的优化方向，可以使用`PoolingAsyncValueTaskMethodBuilder`来池化ValueTask，这样可以减少内存分配。
  - TaskCompletionSource也是可以优化的，后续可以使用`AwaitableCompletionSource`来降低分配。
- 客户端连接池化
  - 目前客户端的连接还是单链接，实际上可以使用连接池来实现，这样可以减少TCP链接的创建和销毁，提高性能。
- 异常场景处理
  - 目前对于服务端和客户端来说，没有详细的测试，针对TCP链接断开，数据包错误，服务器异常等场景的重试，熔断等策略都没有实现。
