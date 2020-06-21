package com.pedometer;

import android.app.Service;
import android.content.Intent;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Binder;
import android.os.IBinder;
import android.os.RemoteException;
import android.util.Log;

import java.util.Arrays;
import java.util.Date;
import java.util.List;

public class StepCounterService extends Service implements SensorEventListener {
    private final String TAG = "emmmm";
    private final String SP_STEP = "emmmm.Steps";
    private final String KEY_TODAY_STEP = "TodaySteps";
    private final String KEY_STEP = "Steps";
    private final String KEY_STEP_TIME = "StepsTime";

    private SensorManager mSensorManager;
    private Sensor mStepSensor;
    private static int sensorTotalStep = 0;
    private static long sensorTotalStepTime = 0;
    private static int todayStep = 0;

    private IBinder mIBinder = new IMyStepCountService.Stub() {
        @Override
        public int getSteps() throws RemoteException {
            return todayStep;
        }
    };

    public StepCounterService() {

    }

    @Override
    public IBinder onBind(Intent intent) {
        return mIBinder;
    }

    @Override
    public void onCreate() {
        super.onCreate();
        mSensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);
        mStepSensor = mSensorManager.getDefaultSensor(Sensor.TYPE_STEP_COUNTER);
        todayStep = getSharedPreferences(SP_STEP, MODE_PRIVATE).getInt(KEY_TODAY_STEP, 0);
        sensorTotalStep = getSharedPreferences(SP_STEP, MODE_PRIVATE).getInt(KEY_STEP, 0);
        sensorTotalStepTime = getSharedPreferences(SP_STEP, MODE_PRIVATE).getLong(KEY_STEP_TIME, 0);
        if (mStepSensor != null) {
            mSensorManager.registerListener(this, mStepSensor, SensorManager.SENSOR_DELAY_NORMAL);
        }

    }

    @Override
    public void onDestroy() {
        mSensorManager.unregisterListener(this);
        super.onDestroy();
    }

    @Override
    public void onSensorChanged(SensorEvent event) {

        //1.The value of the sensor is the cumulative value, and the number of steps from the start to the present is recorded
        //2.The count in the sensor will be lost at shutdown!

        if (event.sensor.getType() == Sensor.TYPE_STEP_COUNTER) {
            int eventSteps = (int) event.values[0];
            //Convert sensor time to current time
            long nowTimestamp = System.currentTimeMillis();

            //If there has been a difference of one day between the time stamps, today's steps = sensor's current steps-recorded current steps.
//            Log.e(TAG, "nowTimestamp / 86400000: 当前时间:" + nowTimestamp / 86400000);

            if (nowTimestamp / 86400000 > sensorTotalStepTime / 86400000) {
                todayStep = sensorTotalStep <= eventSteps ? (int) (eventSteps - sensorTotalStep) : (int) (eventSteps);
            }
            //If it has not been across days, record the increment
            else {
                todayStep += sensorTotalStep <= eventSteps ? (int) (eventSteps - sensorTotalStep) : (int) (eventSteps);
            }

            sensorTotalStepTime = nowTimestamp;
            sensorTotalStep = eventSteps;
            getSharedPreferences(SP_STEP, MODE_PRIVATE)
                    .edit()
                    .putInt(KEY_TODAY_STEP, todayStep)
                    .putInt(KEY_STEP, sensorTotalStep)
                    .putLong(KEY_STEP_TIME, sensorTotalStepTime)
                    .commit();
        }
    }




    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

}
