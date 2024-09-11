using System;
using System.Collections;
using NAudio.Wave;
using UnityEngine;

public class TesteOnStart : MonoBehaviour
{
    private bool stop;
    
    // Start is called before the first frame update
    void Start()
    {
        string outputFilePath = @"C:\Users\spg26\Desktop\NaudioTeste\NaudioTeste\capturedAudio.wav";
    
        // Inicialize a captura do áudio do sistema
        using (var capture = new WasapiLoopbackCapture())
        {
            // Inicialize o gravador de arquivos WaveFileWriter para salvar o áudio capturado
            WaveFileWriter writer = null;
    
            capture.DataAvailable += (s, e) =>
            {
                // Se o escritor ainda não foi inicializado, inicialize com os dados do formato do áudio capturado
                if (writer == null)
                {
                    writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);
                }
    
                // Escreva os dados do áudio capturado no arquivo
                writer.Write(e.Buffer, 0, e.BytesRecorded);
            };
    
            capture.RecordingStopped += (s, e) =>
            {
                // Dispose do escritor e finalize o arquivo corretamente
                writer?.Dispose();
                capture.Dispose();
                Console.WriteLine("Recording stopped and saved to " + outputFilePath);
            };
            
            // Inicie a gravação
            capture.StartRecording();
            while (!stop)
            { }
    
            // Pare a gravação
            capture.StopRecording();
        }
    }

    private void Update()
    {
        if (Input.anyKey) stop = true;
    }
}
