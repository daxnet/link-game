using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace LinkGame.Models
{
    internal sealed class BackgroundMusic : IDisposable
    {
        #region Private Fields

        private readonly List<string> _audioFiles = new();
        private AudioFileReader _audioFileReader;
        private int _curIndex = 0;
        private bool _disposedValue;
        private WaveOutEvent _outputDevice = new();
        private bool _stopRequested = false;

        #endregion Private Fields

        #region Public Constructors

        public BackgroundMusic(IEnumerable<string> audioFiles)
        {
            _audioFiles.AddRange(audioFiles);
            _outputDevice.PlaybackStopped += _outputDevice_PlaybackStopped;
        }

        #endregion Public Constructors

        #region Public Properties

        public float Volume
        {
            get => _outputDevice.Volume;
            set => _outputDevice.Volume = value;
        }

        #endregion Public Properties

        #region Public Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Pause() => _outputDevice.Pause();

        public void Play(bool rewind = false)
        {
            _stopRequested = false;

            if (_outputDevice.PlaybackState == PlaybackState.Playing)
            {
                return;
            }

            if (rewind || _audioFileReader == null)
            {
                _audioFileReader?.Dispose();
                _audioFileReader = new AudioFileReader(_audioFiles[_curIndex]);
                _outputDevice.Init(_audioFileReader);
            }

            _outputDevice.Play();
        }

        public void Stop()
        {
            _stopRequested = true;
            _outputDevice?.Stop();
        }

        #endregion Public Methods

        #region Private Methods

        private void _outputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (!_stopRequested)
            {
                _curIndex = ++_curIndex % _audioFiles.Count;
                _audioFileReader?.Dispose();
                _audioFileReader = new AudioFileReader(_audioFiles[_curIndex]);
                _outputDevice.Init(_audioFileReader);
                _outputDevice.Play();
            }
            else
            {
                _curIndex = 0;
                _audioFileReader?.Dispose();
                _audioFileReader = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _outputDevice?.Stop();
                    _outputDevice?.Dispose();
                    _outputDevice = null;
                    _audioFileReader?.Dispose();
                    _audioFileReader = null;
                }

                _disposedValue = true;
            }
        }

        #endregion Private Methods
    }
}