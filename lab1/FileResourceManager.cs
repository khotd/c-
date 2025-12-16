using System;
using System.IO;

public class FileResourceManager : IDisposable
{
    private FileStream _fileStream;
    private StreamWriter _writer;
    private StreamReader _reader;
    private bool _disposed = false;
    private readonly string _filePath;
    
    public FileResourceManager(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }
    
    // открытие файла для записи
    public void OpenForWriting()
    {
        EnsureNotDisposed();
        
        try
        {
            _fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_fileStream, System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка открытия файла для записи: {ex.Message}");
            throw;
        }
    }
    
    // открытие файла для чтения
    public void OpenForReading()
    {
        EnsureNotDisposed();
        
        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"Файл не найден: {_filePath}");
        
        try
        {
            _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _reader = new StreamReader(_fileStream, System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка открытия файла для чтения: {ex.Message}");
            throw;
        }
    }
    
    // запись строки в файл
    public void WriteLine(string text)
    {
        EnsureNotDisposed();
        
        if (_writer == null)
            throw new InvalidOperationException("Файл не открыт для записи");
        
        try
        {
            _writer.WriteLine(text);
            _writer.Flush();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка записи в файл: {ex.Message}");
            throw;
        }
    }
    
    // чтение всего файла
    public string ReadAllText()
    {
        EnsureNotDisposed();
        
        if (_reader == null)
            throw new InvalidOperationException("Файл не открыт для чтения");
        
        try
        {
            return _reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка чтения файла: {ex.Message}");
            throw;
        }
    }
    
    // добавление текста в конец файла
    public void AppendText(string text)
    {
        EnsureNotDisposed();
        
        try
        {
            using (var streamWriter = new StreamWriter(_filePath, true, System.Text.Encoding.UTF8))
            {
                streamWriter.WriteLine(text);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка добавления текста: {ex.Message}");
            throw;
        }
    }
    
    // получение информации о файле
    public FileInfo GetFileInfo()
    {
        EnsureNotDisposed();
        
        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"Файл не найден: {_filePath}");
        
        try
        {
            return new FileInfo(_filePath);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка получения информации о файле: {ex.Message}");
            throw;
        }
    }
    
    // явное закрытие файла
    public void CloseFile()
    {
        EnsureNotDisposed();
        
        _writer?.Close();
        _reader?.Close();
        _fileStream?.Close();
    }
    
    // проверка на освобождение ресурсов
    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileResourceManager));
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                CloseFile();
                
                _writer?.Dispose();
                _reader?.Dispose();
                _fileStream?.Dispose();
            }
            
            _disposed = true;
        }
    }
    
    ~FileResourceManager()
    {
        Dispose(false);
    }
}

// вспомогательный класс для информации о файле
public class FileInfo
{
    public string Name { get; set; }
    public long Size { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    
    public FileInfo(string filePath)
    {
        var info = new System.IO.FileInfo(filePath);
        Name = info.Name;
        Size = info.Length;
        CreationTime = info.CreationTime;
        LastWriteTime = info.LastWriteTime;
    }
}