package kubemq

import (
	"context"
	"errors"
	"time"

	"github.com/google/uuid"
)

const (
	defaultRequestTimeout = time.Second * 5
)

var (
	ErrNoTransportDefined    = errors.New("no transport layer defined, create object with client instance")
	ErrNoTransportConnection = errors.New("no transport layer established, aborting")
)

type ServerInfo struct {
	Host                string
	Version             string
	ServerStartTime     int64
	ServerUpTimeSeconds int64
}

type Client struct {
	opts                   *Options
	transport              Transport
	ServerInfo             *ServerInfo
	singleStreamQueueMutex chan bool
	//	currentSQM *StreamQueueMessage
}

func generateUUID() string {
	return uuid.New().String()
}

// NewClient - create client instance to be use to communicate with KubeMQ server
func NewClient(ctx context.Context, op ...Option) (*Client, error) {
	opts := GetDefaultOptions()
	for _, o := range op {
		o.apply(opts)
	}
	client := &Client{
		opts: opts,
	}

	err := opts.Validate()
	if err != nil {
		return nil, err
	}
	switch opts.transportType {
	case TransportTypeGRPC:
		client.transport, client.ServerInfo, err = newGRPCTransport(ctx, opts)
	case TransportTypeRest:
		client.transport, client.ServerInfo, err = newRestTransport(ctx, opts)
	}
	if err != nil {
		return nil, err
	}
	if client.transport == nil {
		return nil, ErrNoTransportConnection
	}
	client.singleStreamQueueMutex = make(chan bool, 1)
	return client, nil
}

// Close - closing client connection. any on going transactions will be aborted
func (c *Client) Close() error {
	if c.transport != nil {
		return c.transport.Close()
	}
	return nil
}

// NewEvent - create an empty event
func (c *Client) NewEvent() *Event {
	return c.E()
}

// E - create an empty event object
func (c *Client) E() *Event {
	return &Event{
		Id:        generateUUID(),
		Channel:   c.opts.defaultChannel,
		Metadata:  "",
		Body:      nil,
		ClientId:  c.opts.clientId,
		Tags:      map[string]string{},
		transport: c.transport,
	}
}

// NewEventStore- create an empty event store
func (c *Client) NewEventStore() *EventStore {
	return c.ES()
}

// ES - create an empty event store object
func (c *Client) ES() *EventStore {
	return &EventStore{
		Id:        generateUUID(),
		Channel:   c.opts.defaultChannel,
		Metadata:  "",
		Body:      nil,
		ClientId:  c.opts.clientId,
		Tags:      map[string]string{},
		transport: c.transport,
	}
}

// StreamEvents - send stream of events in a single call
func (c *Client) StreamEvents(ctx context.Context, eventsCh chan *Event, errCh chan error) {
	c.transport.StreamEvents(ctx, eventsCh, errCh)
}

// StreamEventsStore - send stream of events store in a single call
func (c *Client) StreamEventsStore(ctx context.Context, eventsCh chan *EventStore, eventsResultCh chan *EventStoreResult, errCh chan error) {
	c.transport.StreamEventsStore(ctx, eventsCh, eventsResultCh, errCh)
}

// NewCommand - create an empty command
func (c *Client) NewCommand() *Command {
	return c.C()
}

// C - create an empty command object
func (c *Client) C() *Command {
	return &Command{
		Id:        generateUUID(),
		Channel:   c.opts.defaultChannel,
		Metadata:  "",
		Body:      nil,
		Timeout:   defaultRequestTimeout,
		ClientId:  c.opts.clientId,
		Tags:      map[string]string{},
		transport: c.transport,
		trace:     nil,
	}
}

// NewQuery - create an empty query
func (c *Client) NewQuery() *Query {
	return c.Q()
}

// Q - create an empty query object
func (c *Client) Q() *Query {
	return &Query{
		Id:        generateUUID(),
		Channel:   c.opts.defaultChannel,
		Metadata:  "",
		Body:      nil,
		Timeout:   defaultRequestTimeout,
		ClientId:  c.opts.clientId,
		CacheKey:  "",
		CacheTTL:  c.opts.defaultCacheTTL,
		Tags:      map[string]string{},
		transport: c.transport,
		trace:     nil,
	}
}

// NewResponse - create an empty response
func (c *Client) NewResponse() *Response {
	return c.R()
}

// R - create an empty response object for command or query responses
func (c *Client) R() *Response {
	return &Response{
		RequestId:  "",
		ResponseTo: "",
		Metadata:   "",
		Body:       nil,
		ClientId:   c.opts.clientId,
		ExecutedAt: time.Time{},
		Err:        nil,
		Tags:       map[string]string{},
		transport:  c.transport,
		trace:      nil,
	}
}

// SubscribeToEvents - subscribe to events by channel and group. return channel of events or en error
func (c *Client) SubscribeToEvents(ctx context.Context, channel, group string, errCh chan error) (<-chan *Event, error) {
	return c.transport.SubscribeToEvents(ctx, channel, group, errCh)
}

// SubscribeToEventsStore - subscribe to events store by channel and group with subscription option. return channel of events or en error
func (c *Client) SubscribeToEventsStore(ctx context.Context, channel, group string, errCh chan error, opt SubscriptionOption) (<-chan *EventStoreReceive, error) {
	return c.transport.SubscribeToEventsStore(ctx, channel, group, errCh, opt)
}

// SubscribeToCommands - subscribe to commands requests by channel and group. return channel of CommandReceived or en error
func (c *Client) SubscribeToCommands(ctx context.Context, channel, group string, errCh chan error) (<-chan *CommandReceive, error) {
	return c.transport.SubscribeToCommands(ctx, channel, group, errCh)
}

// SubscribeToQueries - subscribe to queries requests by channel and group. return channel of QueryReceived or en error
func (c *Client) SubscribeToQueries(ctx context.Context, channel, group string, errCh chan error) (<-chan *QueryReceive, error) {
	return c.transport.SubscribeToQueries(ctx, channel, group, errCh)
}

// NewQueueMessage - create an empty queue messages
func (c *Client) NewQueueMessage() *QueueMessage {
	return c.QM()
}

// QM - create an empty queue message object
func (c *Client) QM() *QueueMessage {
	return &QueueMessage{
		Id:         "",
		ClientId:   c.opts.clientId,
		Channel:    "",
		Metadata:   "",
		Body:       nil,
		Tags:       map[string]string{},
		Attributes: nil,
		Policy: &QueueMessagePolicy{
			ExpirationSeconds: 0,
			DelaySeconds:      0,
			MaxReceiveCount:   0,
			MaxReceiveQueue:   "",
		},
		transport: c.transport,
		trace:     nil,
	}
}

// NewQueueMessages - create an empty queue messages array
func (c *Client) NewQueueMessages() *QueueMessages {
	return c.QMB()
}

// QMB - create an empty queue message array object
func (c *Client) QMB() *QueueMessages {
	return &QueueMessages{
		Messages:  []*QueueMessage{},
		transport: c.transport,
	}
}

// SendQueueMessage - send single queue message
func (c *Client) SendQueueMessage(ctx context.Context, msg *QueueMessage) (*SendQueueMessageResult, error) {
	return c.transport.SendQueueMessage(ctx, msg)
}

// SendQueueMessages - send multiple queue messages
func (c *Client) SendQueueMessages(ctx context.Context, msg []*QueueMessage) ([]*SendQueueMessageResult, error) {
	return c.transport.SendQueueMessages(ctx, msg)
}

// NewReceiveQueueMessagesRequest - create an empty receive queue message request object
func (c *Client) NewReceiveQueueMessagesRequest() *ReceiveQueueMessagesRequest {
	return c.RQM()
}

// RQM - create an empty receive queue message request object
func (c *Client) RQM() *ReceiveQueueMessagesRequest {
	return &ReceiveQueueMessagesRequest{
		RequestID:           "",
		ClientID:            c.opts.clientId,
		Channel:             "",
		MaxNumberOfMessages: 0,
		WaitTimeSeconds:     0,
		IsPeak:              false,
		transport:           c.transport,
		trace:               nil,
	}
}

// ReceiveQueueMessages - call to receive messages from a queue
func (c *Client) ReceiveQueueMessages(ctx context.Context, req *ReceiveQueueMessagesRequest) (*ReceiveQueueMessagesResponse, error) {
	return c.transport.ReceiveQueueMessages(ctx, req)
}

// NewAckAllQueueMessagesRequest - create an empty ack all receive queue messages request object
func (c *Client) NewAckAllQueueMessagesRequest() *AckAllQueueMessagesRequest {
	return c.AQM()
}

// AQM - create an empty ack all receive queue messages request object
func (c *Client) AQM() *AckAllQueueMessagesRequest {
	return &AckAllQueueMessagesRequest{
		RequestID:       "",
		ClientID:        c.opts.clientId,
		Channel:         "",
		WaitTimeSeconds: 0,
		transport:       c.transport,
		trace:           nil,
	}
}

// AckAllQueueMessages - send ack all messages in queue
func (c *Client) AckAllQueueMessages(ctx context.Context, req *AckAllQueueMessagesRequest) (*AckAllQueueMessagesResponse, error) {
	return c.transport.AckAllQueueMessages(ctx, req)
}

// NewStreamQueueMessage - create an empty stream receive queue message object
func (c *Client) NewStreamQueueMessage() *StreamQueueMessage {
	return c.SQM()
}

// SQM - create an empty stream receive queue message object
func (c *Client) SQM() *StreamQueueMessage {

	c.singleStreamQueueMutex <- true
	sqm := &StreamQueueMessage{
		RequestID:         "",
		ClientID:          c.opts.clientId,
		Channel:           "",
		visibilitySeconds: 0,
		waitTimeSeconds:   0,
		refSequence:       0,
		reqCh:             nil,
		resCh:             nil,
		errCh:             nil,
		doneCh:            nil,
		msg:               nil,
		transport:         c.transport,
		trace:             nil,
		ctx:               nil,
		releaseCh:         c.singleStreamQueueMutex,
	}
	return sqm
}
