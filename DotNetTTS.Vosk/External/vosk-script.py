# -*- coding: cp1251 -*-
from vosk_tts import Model, Synth
import sys
import os

if __name__ == '__main__':
    sys.stdin.reconfigure(encoding='utf-8')
    sys.stdout.reconfigure(encoding='utf-8')
    sys.stderr.reconfigure(encoding='utf-8')

    model_name = sys.argv[1]

    model = Model(model_name=model_name)
    synth = Synth(model)

    index = 0

    #warmup
    synth.synth('Карл у Клары украл кораллы, Клара у Карла украла кларнет', 'temp.wav')

    os.remove('temp.wav')

    print("[READY]")
    sys.stdout.flush();

    out = sys.stdout
    sys.stdout = sys.stderr

    for line in sys.stdin:
        result_file_name = f'result{index}.wav';

        synth.synth(line, result_file_name)
        print(result_file_name, file=out)
        out.flush();
        index += 1
