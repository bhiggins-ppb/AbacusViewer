namespace AbacusViewer.Services
{
    //TODO: I have a feeling that this class can be avoided by using the Singleton Autofac registry technique - maybe investigate more about this 
    // The singleton factory pattern uses a static Instance to keep the singleton reference: http://csharpindepth.com/Articles/General/Singleton.aspx
    // Application_Start (where we call the Autofac code) in global.asax is called only once at first request and does not re-trigger with app pool recycle. 

    public sealed class SingletonEventCollectorFactory
    {
        private readonly EventCollector _eventCollector;
        public EventCollector Instance => _eventCollector;

        public SingletonEventCollectorFactory(//GoogleProtocolBuffersGzipSerializer googleSerializer, 
            int maxQueueDepth//, KafkaConfig kafkaConfig
                                                                                                                     )
        {
            //_eventCollector = new EventCollector(new EventConsumer(googleSerializer, kafkaConfig), maxQueueDepth);

            //_eventCollector.Start();
        }
    }
}