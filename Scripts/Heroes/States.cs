
using System;

public enum States {
    [AnimationPriority(0)] IDLE,
    [AnimationPriority(1)] RUN,
    [AnimationPriority(2)] JUMP,
    [AnimationPriority(2)] FALL,
    [AnimationPriority(3)] ATTACK1,
    [AnimationPriority(3)] ATTACK2,
    [AnimationPriority(4)] TAKE_HIT,
    [AnimationPriority(5)] DIE,
    [AnimationPriority(2)] DEAD,
    [AnimationPriority(3)] SKILL1,
    [AnimationPriority(3)] SKILL2
}

// Атрибут для хранения приоритета анимации
public class AnimationPriorityAttribute : Attribute
{
    public int Priority { get; }

    public AnimationPriorityAttribute(int priority)
    {
        Priority = priority;
    }
}

// Класс для получения приоритета
public static class StatesExtensions
{
    public static int GetPriority(this States state)
    {
        var fieldInfo = state.GetType().GetField(state.ToString());
        var attribute = (AnimationPriorityAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(AnimationPriorityAttribute));
        return attribute?.Priority ?? 0; // Возвращаем 0, если атрибут не найден
    }
}