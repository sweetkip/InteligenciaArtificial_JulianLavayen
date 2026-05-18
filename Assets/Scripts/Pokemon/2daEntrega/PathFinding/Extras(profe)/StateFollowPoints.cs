/*using System.Collections.Generic;
using UnityEngine;

public class StateFollowPoints<T> : State<T>
{
    protected List<Vector3> _waypoints;
    int _index;
    protected Transform _entity;
    float _distanceToPoint = 0.2f;
    bool _isFinishPath;

    public StateFollowPoints(Transform entity, float distanceToPoint = 0.2f)
    {
        _entity = entity;
        _distanceToPoint = distanceToPoint;
        _isFinishPath = true;
    }
    public StateFollowPoints(Transform entity, List<Vector3> waypoints, float distanceToPoint = 0.2f)
    {
        _entity = entity;
        _distanceToPoint = distanceToPoint;
        _waypoints = waypoints;
        _isFinishPath = true;
    }

    public override void Execute()
    {
        base.Execute();
        Run();
    }
    public void SetWaypoints(List<Vector3> newPoints)
    {
        if (newPoints.Count == 0) return;
        _waypoints = newPoints;
        _index = 0;
        _isFinishPath = false;
        OnStartPath();
    }
    void Run()
    {
        if (_isFinishPath) return;
        Vector3 point = _waypoints[_index];
        point.y = _entity.position.y;
        Vector3 dir = point - _entity.position;
        if (dir.magnitude < _distanceToPoint)
        {
            if (_index + 1 < _waypoints.Count)
                _index++;
            else
            {
                _isFinishPath = true;
                OnFinishPath();
                return;
            }
        }
        OnMove(dir.normalized);
    }
    protected virtual void OnMove(Vector3 dir) {}
    protected virtual void OnStartPath() {}
    protected virtual void OnFinishPath() {}
    public bool IsFinishPath => _isFinishPath;
}*/