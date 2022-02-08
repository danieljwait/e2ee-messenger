﻿using Caliburn.Micro;
using MessengerAppClient.Content.ViewModels;
using MessengerAppClient.Login.ViewModels;
using MessengerAppClient.Shell.Messages;
using MessengerAppClient.Shell.Models;
using MessengerAppShared;
using MessengerAppShared.Messages;
using System;
using System.Threading;
using System.Windows;

namespace MessengerAppClient.Shell.ViewModels
{
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive,
        IHandle<InternalClientMessage>, IHandle<ServerToClientMessage>
    {
        // Socket for networking
        private SocketHandler _socketHandler;

        // Handle prevents program from closing before necessary routines finish
        private AutoResetEvent _shutdownWaitHandle = new AutoResetEvent(false);

        // Event Aggregator instance
        private readonly IEventAggregator _eventAggregator;

        // Screen Collection
        private readonly LoginConductorViewModel _loginConductorViewModel;
        private readonly ContentConductorViewModel _contentConductorViewModel;

        // Constructor
        public ShellViewModel(
            IEventAggregator eventAggregator,
            LoginConductorViewModel loginConductorViewModel,
            ContentConductorViewModel contentConductorViewModel)
        {
            // Dependency injection
            _eventAggregator = eventAggregator;
            _loginConductorViewModel = loginConductorViewModel;
            _contentConductorViewModel = contentConductorViewModel;

            // Creating server connection
            _socketHandler = new SocketHandler();

            // Try connect to server, if fail close program
            try
            {
                _socketHandler.Connect();
            }
            catch(Exception exception)
            {
                // Shut down program, report server disabled error to Windows
                FatalError(exception, 1341);
            }

            // Begin receiving from server
            _socketHandler.ReceiveLoop(ReceiveLoopCallback);
        }

        // Ends the receive; publish message; calls ReceiveLoop to start again
        private void ReceiveLoopCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Gets object that was received
                object received_object = Protocol.GetObject(asyncResult, _socketHandler.Buffer);

                // Publishes message that a new SocketServerMessage has been received
                _eventAggregator.PublishOnUIThread((ServerToClientMessage)received_object);

                // Continues infinite receive loop
                _socketHandler.ReceiveLoop(ReceiveLoopCallback);
            }
            catch (Exception exception)
            {
                // Shut down program, report unknown network error to Windows
                FatalError(exception, 59);
            }
        }

        // Tell server about disconnect
        private void DisconnectRoutine()
        {
            // Directly sends disconnect notice to server
            var ServerMessage = new ClientToServerMessage(ClientCommand.Disconnect);
            _socketHandler.SendToServer(ServerMessage);
            // Close socket
            _socketHandler.Disconnect();

            _shutdownWaitHandle.Set();
        }

        // Tell server about log out and change screen
        private void LogOutRoutine()
        {
            // Sends log out notice to server
            var ServerMessage = new ClientToServerMessage(ClientCommand.LogOut);
            var InternalMessage = new InternalClientMessage(InternalClientCommand.SendMessage, ServerMessage);
            _eventAggregator.PublishOnUIThread(InternalMessage);

            // Goes back to login screen
            ActivateItem(_loginConductorViewModel);
        }

        // Handle internal
        public void Handle(InternalClientMessage message)
        {
            // Switch on the Command enum in the message
            switch (message.Command)
            {
                case InternalClientCommand.Connect:
                    _socketHandler.Connect();
                    break;

                case InternalClientCommand.Disconnect:
                    DisconnectRoutine();
                    break;

                case InternalClientCommand.LogOut:
                    LogOutRoutine();
                    break;

                case InternalClientCommand.ValidLogin:
                    ValidLoginRoutine(message.Data);
                    break;

                case InternalClientCommand.SendMessage:
                    _socketHandler.SendToServer(message.Data);
                    break;
            }
        }

        // Handle from server
        public void Handle(ServerToClientMessage message)
        {
            InternalClientMessage InternalMessage;

            switch (message.Command)
            {
                case ServerCommand.LoginResult:
                    LoginResultRoutine(message.Data);
                    break;

                case ServerCommand.ClientConnect:
                    InternalMessage = new InternalClientMessage(InternalClientCommand.ClientConnect, message.Data);
                    _eventAggregator.PublishOnUIThread(InternalMessage);
                    break;

                case ServerCommand.ClientDisconnect:
                    InternalMessage = new InternalClientMessage(InternalClientCommand.ClientDisconnect, message.Data);
                    _eventAggregator.PublishOnUIThread(InternalMessage);
                    break;

                case ServerCommand.SendClientsList:
                    InternalMessage = new InternalClientMessage(InternalClientCommand.ReceiveClientList, message.Data);
                    _eventAggregator.PublishOnUIThread(InternalMessage);
                    break;

                case ServerCommand.Message:
                    InternalMessage = new InternalClientMessage(InternalClientCommand.ReceiveMessage, message.Data);
                    _eventAggregator.PublishOnUIThread(InternalMessage);
                    break;
            }
        }

        // Pass control to content Conductor and publish login details
        private void ValidLoginRoutine(object details_object)
        {
            // Pass control over to content Conductor from login Conductor
            ActivateItem(_contentConductorViewModel);

            var InternalMessage = new InternalClientMessage(InternalClientCommand.LoginDetails, details_object);
            _eventAggregator.PublishOnUIThread(InternalMessage);
        }

        // Determine if login attempt was success
        private void LoginResultRoutine(object data)
        {
            // When login is invalid, data is false
            if (data is false)
            {
                return;
            }

            // Notify of valid login
            var InternalMessage = new InternalClientMessage(InternalClientCommand.ValidLogin, data);
            _eventAggregator.PublishOnUIThread(InternalMessage);
        }

        // When the [X] button is pressed, disconnect socket first
        public override void CanClose(Action<bool> callback)
        {
            try
            {
                // Send log out notice to server
                var InternalMessage = new InternalClientMessage(InternalClientCommand.Disconnect);
                _eventAggregator.PublishOnUIThread(InternalMessage);

                // Pause closing until handle is set by completing the send to the server
                _shutdownWaitHandle.WaitOne();
            }
            catch { }

            // Closes the window
            base.CanClose(callback);
        }

        // When a fatal error is thrown, close program
        private void FatalError(Exception exception, int error_code=0)
        {
            MessageBox.Show($"Error: {exception.Message}\n\nPress OK to close the program", "A fatal error was caught");
            
            // Close window
            TryClose();

            // Close program
            Environment.Exit(1341);
        }

        // When window opened, begin listening to EventAggregator and show LoginConductorViewModel
        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            ActivateItem(_loginConductorViewModel);
        }

        // When window closed, stop listening to EventAggregator
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);
        }
    }
}