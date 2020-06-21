package com.pedometer.ui;

import androidx.appcompat.app.AppCompatActivity;
import androidx.databinding.DataBindingUtil;

import android.content.ComponentName;
import android.content.Intent;
import android.content.ServiceConnection;
import android.graphics.Color;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.IBinder;
import android.os.RemoteException;
import android.view.Menu;
import android.view.MenuItem;

import com.github.mikephil.charting.charts.LineChart;
import com.github.mikephil.charting.components.Description;
import com.github.mikephil.charting.components.Legend;
import com.github.mikephil.charting.components.XAxis;
import com.github.mikephil.charting.data.Entry;
import com.github.mikephil.charting.data.LineData;
import com.github.mikephil.charting.data.LineDataSet;
import com.pedometer.BR;
import com.pedometer.IMyStepCountService;
import com.pedometer.MyApplication;
import com.pedometer.R;
import com.pedometer.StepCounterService;
import com.pedometer.client.ApiException;
import com.pedometer.client.api.StepInfoesApi;
import com.pedometer.client.model.StepInfo;
import com.pedometer.client.model.UserInfo;
import com.pedometer.databinding.*;
import com.pedometer.ui.login.LoginActivity;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.Date;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeoutException;


public class MainActivity extends AppCompatActivity {
    private int steps = 0;
    private StepInfo[] stepInfos;
    private com.github.mikephil.charting.charts.LineChart stepsChart;
    private com.github.mikephil.charting.charts.LineChart distanceChart;
    private com.github.mikephil.charting.charts.LineChart caloriesChart;
    private ActivityMainBindingImpl binding;
    private IMyStepCountService mAidl;
    private Timer timer;
    private ServiceConnection mConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName name, IBinder service) {
            //After connecting, get Binder, convert to AIDL, and return to an agent in different processes
            mAidl = IMyStepCountService.Stub.asInterface(service);
        }

        @Override
        public void onServiceDisconnected(ComponentName name) {
            mAidl = null;
        }
    };

    private static int compare(StepInfo a, StepInfo b) {
        StepInfo sa = a;
        StepInfo sb = b;
        return (int) (sa.getTimeStamp() - sb.getTimeStamp());
    }


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        binding = DataBindingUtil.setContentView(this, R.layout.activity_main);

        stepsChart = findViewById(R.id.main_activity_steps_chart);
        distanceChart = findViewById(R.id.main_activity_distance_chart);
        caloriesChart = findViewById(R.id.main_activity_calories_chart);

        initCharts(stepsChart);
        initCharts(distanceChart);
        initCharts(caloriesChart);

        Intent it = new Intent(getApplicationContext(), StepCounterService.class);
        startService(it);
        Intent intent1 = new Intent(getApplicationContext(), StepCounterService.class);
        bindService(intent1, mConnection, BIND_AUTO_CREATE);

        AsyncTask.execute(this::loadChartDatas);

        timer = new Timer();
        timer.schedule(new TimerTask() {
            @Override
            public void run() {
                uploadData();
            }
        }, 1000, 10000);

        refreshStepInfo();
    }

    private SimpleDateFormat sdf = new SimpleDateFormat("M-d");

    private void initCharts(LineChart chart) {
        Description description = new Description();
        description.setEnabled(false);
        chart.setDescription(description);
        chart.setDrawGridBackground(false);
        chart.setDrawBorders(true);

        XAxis xAxis = chart.getXAxis();
        xAxis.setValueFormatter((v, axisBase) -> sdf.format(new Date((long) v)));

        Legend legend = chart.getLegend();
        //Set the display type, LINE CIRCLE SQUARE EMPTY, etc. Various ways, just check LegendForm
        legend.setForm(Legend.LegendForm.LINE);
        legend.setTextSize(12f);
        //Display position lower left
        legend.setVerticalAlignment(Legend.LegendVerticalAlignment.BOTTOM);
        legend.setHorizontalAlignment(Legend.LegendHorizontalAlignment.LEFT);
        legend.setOrientation(Legend.LegendOrientation.HORIZONTAL);
        //Whether to draw in the chart
        legend.setDrawInside(false);
        legend.setEnabled(false);
    }

    private LineData initLineDataSet(List<Entry> entrys, String label) {
        LineDataSet lineDataSet = new LineDataSet(entrys, label);
        lineDataSet.setLineWidth(1f);
        lineDataSet.setCircleRadius(3f);
        //Set whether the dot of the curve value is solid or hollow
        lineDataSet.setDrawCircleHole(false);
        lineDataSet.setValueTextSize(10f);
        //Set line chart fill
        lineDataSet.setDrawFilled(true);
        lineDataSet.setFormLineWidth(1f);
        lineDataSet.setFormSize(15.f);
        lineDataSet.setDrawValues(false);
        //The setting curve is displayed as a smooth curve (if not set, the default polyline)
        lineDataSet.setMode(LineDataSet.Mode.CUBIC_BEZIER);

        return new LineData(lineDataSet);
    }

    private int getDistance(int steps) {
        return (int) (steps * MyApplication.UserInfo.getHeight() * 0.01 * 0.37);
    }

    private int getCalories(int steps) {
        return (int) (steps * (MyApplication.UserInfo.getWeight() / 2000));
    }

    private void loadChartDatas() {
        StepInfoesApi api = new StepInfoesApi();
        //Get exercise data for nearly 30 days
        try {
            stepInfos = api.stepInfoesGet(System.currentTimeMillis() - 2592000000L - System.currentTimeMillis() % 86400000L).toArray(new StepInfo[0]);
            if (stepInfos == null || stepInfos.length == 0) {
                return;
            }
            Arrays.sort(stepInfos, MainActivity::compare);

            ArrayList<Entry> stepEntrys = new ArrayList<>();
            ArrayList<Entry> distanceEntrys = new ArrayList<>();
            ArrayList<Entry> caloriesEntrys = new ArrayList<>();

            for (int i = 0; i < stepInfos.length; i++) {
                StepInfo info = stepInfos[i];
                stepEntrys.add(new Entry(info.getTimeStamp(), info.getTodaySteps()));
                distanceEntrys.add(new Entry(info.getTimeStamp(), getDistance(info.getTodaySteps())));
                caloriesEntrys.add(new Entry(info.getTimeStamp(), getCalories(info.getTodaySteps())));
            }
            runOnUiThread(() -> {
                stepsChart.setData(initLineDataSet(stepEntrys, "Steps"));
                distanceChart.setData(initLineDataSet(distanceEntrys, "Distance"));
                caloriesChart.setData(initLineDataSet(caloriesEntrys, "Calories"));

                stepsChart.invalidate();
                distanceChart.invalidate();
                caloriesChart.invalidate();
            });
        } catch (Exception e) {
            e.printStackTrace();
        }

    }

    @Override
    protected void onDestroy() {
        unbindService(mConnection);
        timer.cancel();
        super.onDestroy();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_settings:
                startActivity(new Intent(this, UserProfileActivity.class));
                return true;
            case R.id.action_logout:
                startActivity(new Intent(this, LoginActivity.class));
                finish();
                return true;
            default:
                return false;
        }

    }

    private void refreshStepInfo() {
        Timer rfTimer = new Timer();
        rfTimer.schedule(new TimerTask() {
            @Override
            public void run() {
                try {
                    steps = mAidl.getSteps();
                    binding.setSteps(steps + " :step");
                    binding.setCalories(getCalories(steps) + "cal");
                    binding.setDistance(getDistance(steps) + "m");
                    binding.invalidateAll();
                } catch (RemoteException e) {
                    e.printStackTrace();
                }
            }
        }, 1000, 1000);
    }

    /**
     * Upload the current steps to the server.
     */
    private void uploadData() {

        try {
            StepInfo stepInfo = new StepInfo();
            stepInfo.setId(0);
            stepInfo.setTimeStamp(System.currentTimeMillis());
            stepInfo.setUserId(MyApplication.UserInfo.getId());
            stepInfo.setTodaySteps(steps);
            stepInfo.setDistance((float) getDistance(steps));//步长=身高*0.37 单位：米。身高单位：厘米
            stepInfo.setCalories((float) getCalories(steps));//卡路里，总的能量 = (体重÷2000 )*总步数

            StepInfoesApi api = new StepInfoesApi();
            api.stepInfoesPostStepInfo(stepInfo);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
