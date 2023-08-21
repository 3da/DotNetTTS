# -*- coding: cp1251 -*-
import torch
import sys
import os


if __name__ == '__main__':
    sys.stdin.reconfigure(encoding='utf-8')
    sys.stdout.reconfigure(encoding='utf-8')
    sys.stderr.reconfigure(encoding='utf-8')

    speaker = sys.argv[1]
    sample_rate = int(sys.argv[2])
    model_path = sys.argv[3]

    device = torch.device('cpu')
    torch.set_num_threads(4)

    model = torch.package.PackageImporter(model_path).load_pickle("tts_models", "model")
    model.to(device)

    index = 0

    #warmup
    model.save_wav(text='Карл у Клары украл кораллы, Клара у Карла украла кларнет',
                            speaker=speaker,
                            sample_rate=sample_rate,
                            audio_path='temp.wav')

    os.remove('temp.wav')

    print("[READY]")
    sys.stdout.flush();

    out = sys.stdout
    sys.stdout = sys.stderr

    for line in sys.stdin:
        result_file_name = f'result{index}.wav';

        audio = model.save_wav(text=line,
                            speaker=speaker,
                            sample_rate=sample_rate,
                            audio_path=result_file_name)

        print(result_file_name, file=out)
        out.flush();
        index += 1