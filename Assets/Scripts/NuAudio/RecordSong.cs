using NAudio.Wave;
using UnityEngine;

public class RecordSong : MonoBehaviour
{
    private WasapiLoopbackCapture capture;
    private WaveFileWriter writer;
    private string outputFilePath = @"C:\Users\spg26\Desktop\NaudioTeste\capturedAudio.wav";
    private bool isRecording;
    
    public void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }
    
    private void StartRecording()
    {
        // Inicialize a captura do áudio do sistema
        capture = new WasapiLoopbackCapture();

        // Inicialize o gravador de arquivos WaveFileWriter
        writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);

        // Inscreva-se nos eventos de captura de áudio
        capture.DataAvailable += OnDataAvailable;
        capture.RecordingStopped += OnRecordingStopped;

        // Inicie a gravação
        capture.StartRecording();
        Debug.Log("Gravação iniciada...");
        isRecording = true;
    }
    
    private void StopRecording()
    {
        // Pare a gravação
        if (capture != null && isRecording)
        {
            capture.StopRecording();
            Debug.Log("Gravação parada...");
            isRecording = false;
        }
    }

    // Callback para quando dados de áudio estão disponíveis
    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        if (writer != null)
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
        }
    }

    // Callback para quando a gravação é interrompida
    private void OnRecordingStopped(object sender, StoppedEventArgs e)
    {
        // Dispose dos recursos adequadamente
        if (writer != null)
        {
            writer.Dispose();
            writer = null;
        }

        if (capture != null)
        {
            capture.Dispose();
            capture = null;
        }

        Debug.Log("Gravação salva em: " + outputFilePath);
    }

    private void OnApplicationQuit()
    {
        // Certifique-se de liberar os recursos quando o aplicativo for fechado
        if (isRecording)
        {
            StopRecording();
        }
    }
}
