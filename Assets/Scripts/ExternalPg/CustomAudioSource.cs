using Agora.Rtc;

public class CustomAudioSource : IAudioFrameObserver
{
    private float[] _audioBuffer;
    private int _sampleRate = 44100;

    public CustomAudioSource(int bufferSize)
    {
        _audioBuffer = new float[bufferSize];
    }

    // Esta função será chamada pelo Agora para obter os dados de áudio
    public void OnRecordFrame(float[] data, int samples)
    {
        // Preencha o buffer de áudio com seus dados
        System.Array.Copy(data, _audioBuffer, samples);
    }

    public float[] GetAudioBuffer()
    {
        return _audioBuffer;
    }

    // Implementar outros métodos do IAudioFrameObserver conforme necessário
    public void OnPlaybackFrame(float[] data, int samples) { }
    public void OnMixedFrame(float[] data, int samples) { }
}
